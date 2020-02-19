﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

namespace MxNet.Optimizers
{
    public class OptimState : NDArrayDict
    {
        
    }

    public abstract class Optimizer
    {
        public float LearningRate
        {
            get
            {
                if (Scheduler != null)
                    return Scheduler.Call(NumUpdate);
                else
                    return lr;
            }
        }

        public float WD { get; set; }
        public float? ClipGradient { get; set; }
        public float RescaleGrad { get; set; }
        public LRScheduler Scheduler { get; set; }
        public bool MultiPrecision { get; set; }
        public uint BeginNumUpdate { get; set; }
        public uint NumUpdate { get; set; }
        public int AggregateNum { get; set; }
        public Dictionary<int, string> Idx2Name { get; set; }
        public Dictionary<int, Gluon.Parameter> ParamDict { get; set; }

        private float lr;
        private Dictionary<string, float> lr_mult = new Dictionary<string, float>();
        private Dictionary<string, float> wd_mult = new Dictionary<string, float>();
        private Dictionary<int, Dictionary<int, int>> all_index_update_counts = new Dictionary<int, Dictionary<int, int>>();
        private Dictionary<int, int> index_update_count = new Dictionary<int, int>();
        private (Dictionary<string, Dictionary<string, string>>, List<string>) sym_info;

        private Dictionary<string, Optimizer> opt_registry = new Dictionary<string, Optimizer>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Optimizer"/> class.
        /// </summary>
        /// <param name="lr">The lr.</param>
        /// <param name="name">The name.</param>
        public Optimizer(float rescale_grad= 1, Dictionary<int, string> param_idx2name= null, float wd= 0,
                        float? clip_gradient= null, float learning_rate= 0.01f, LRScheduler lr_scheduler= null,
                        Symbol sym= null, uint begin_num_update= 0, bool multi_precision= false, Dictionary<int, Gluon.Parameter> param_dict= null)
        {
            lr = learning_rate;
            RescaleGrad = rescale_grad;
            Scheduler = lr_scheduler;
            if (Scheduler != null)
                Scheduler.BaseLearningRate = learning_rate;

            WD = wd;
            BeginNumUpdate = begin_num_update;
            NumUpdate = begin_num_update;
            all_index_update_counts.Add(0, new Dictionary<int, int>());
            index_update_count = all_index_update_counts[0];
            ClipGradient = clip_gradient;
            MultiPrecision = multi_precision;
            AggregateNum = 0;
            if (param_idx2name == null)
                param_idx2name = new Dictionary<int, string>();

            Idx2Name = param_idx2name;
            if(sym !=null)
            {
                sym_info = (sym.ListAttributeDict(), sym.ListArguments().ToList());
            }
            else
            {
                sym_info = new ValueTuple<Dictionary<string, Dictionary<string, string>>, List<string>>(new Dictionary<string, Dictionary<string, string>>(), new List<string>());
            }

            if (param_dict != null)
                ParamDict = param_dict;
            else
                ParamDict = new Dictionary<int, Gluon.Parameter>();

            SetLrMult(new Dictionary<string, float>());
            SetWdMult(new Dictionary<string, float>());
        }

        public abstract NDArrayDict CreateState(int index, NDArray weight);

        public virtual (NDArrayDict, NDArray) CreateStateMultiPrecision(int index, NDArray weight)
        {
            NDArray weight_master_copy = null;
            if (MultiPrecision && weight.DataType.Name == DType.Float16.Name)
            {
                weight_master_copy = weight.AsType(DType.Float32);
                return (CreateState(index, weight_master_copy), weight_master_copy);
            }

            if (!MultiPrecision && weight.DataType.Name == DType.Float16.Name)
                Logger.Warning("Accumulating with float16 in optimizer can lead to " +
                          "poor accuracy or slow convergence. " +
                          "Consider using multi_precision=True option of the " +
                          "optimizer");

            return (CreateState(index, weight), weight);
        }

        public abstract void Update(int index, NDArray weight, NDArray grad, NDArrayDict state);

        public virtual void UpdateMultiPrecision(int index, NDArray weight, NDArray grad, (NDArrayDict, NDArray) state)
        {
            if (MultiPrecision && weight.DataType.Name == DType.Float16.Name)
            {
                var weight_master_copy = state.Item2;
                var grad32 = grad.AsType(DType.Float32);
                Update(index, weight_master_copy, grad32, state.Item1);
                weight = weight_master_copy.Cast(weight.DataType);
            }
            else
            {
                Update(index, weight, grad, state.Item1);
            }
        }

        public void SetLearningRate(float lr)
        {
            Logger.Warning("[DEPRECATED] Sets lr scale. Use SetLrMult instead");
        }

        public static Updater GetUpdater(Optimizer optimizer)
        {
            return optimizer.GetUpdater();
        }

        internal void SetLrMult(Dictionary<string, float> args_lr_mult)
        {
            lr_mult = new Dictionary<string, float>();
            if(sym_info.Item1.Count > 0)
            {
                var (attr, arg_names) = this.sym_info;
                foreach (var name in arg_names)
                {
                    if(attr.ContainsKey(name) && attr[name].ContainsKey("__lr_mult__"))
                    {
                        if(float.TryParse(attr[name]["__lr_mult__"], out var attrValue))
                            lr_mult[name] = attrValue;
                    }
                }
            }

            foreach (var item in args_lr_mult)
            {
                lr_mult[item.Key] = item.Value;
            }
        }

        internal void SetWdMult(Dictionary<string, float> args_wd_mult)
        {
            wd_mult = new Dictionary<string, float>();
            foreach (var n in Idx2Name.Values)
            {
                if (!n.EndsWith("_weight") || n.EndsWith("_gamma"))
                    wd_mult[n] = 0;
            }

            if (sym_info.Item1.Count > 0)
            {
                var (attr, arg_names) = this.sym_info;
                foreach (var name in arg_names)
                {
                    if (attr.ContainsKey(name) && attr[name].ContainsKey("__wd_mult__"))
                    {
                        if (float.TryParse(attr[name]["__wd_mult__"], out var attrValue))
                            wd_mult[name] = attrValue;
                    }
                }
            }

            foreach (var item in args_wd_mult)
            {
                wd_mult[item.Key] = item.Value;
            }
        }

        internal void SetCurrentContext(int device_id)
        {
            if(all_index_update_counts.ContainsKey(device_id))
            {
                all_index_update_counts[device_id] = new Dictionary<int, int>();
            }

            index_update_count = all_index_update_counts[device_id];
        }

        internal void UpdateCount(int[] index)
        {
            foreach (var idx in index)
            {
                if (!index_update_count.ContainsKey(idx))
                    index_update_count[idx] = (int)BeginNumUpdate;

                index_update_count[idx] += 1;
                NumUpdate = (uint)Math.Max(index_update_count[idx], NumUpdate);
            }
        }

        internal float[] GetLrs(int[] indices)
        {
            float lr = 0;
            if (Scheduler != null)
                lr = Scheduler.Call(NumUpdate);
            else
                lr = LearningRate;

            float[] lrs = new float[indices.Length];
            for(int i=0;i<indices.Length;i++)
            {
                int index = indices[i];
                lrs[i] = lr;
                if (ParamDict.ContainsKey(index))
                    lrs[i] *= ParamDict[index].Lr_Mult;
                else if (lr_mult.ContainsKey(index.ToString()))
                    lrs[i] *= lr_mult[index.ToString()];
                else if (Idx2Name.ContainsKey(index))
                {
                    float Idx2Name_lrvalue = 1;
                    if(float.TryParse(Idx2Name[index], out Idx2Name_lrvalue))
                    {
                        lrs[i] *= Idx2Name_lrvalue;
                    }
                }
            }

            return lrs;
        }

        internal float GetLr(int index) => GetLrs(new int[] { index })[0];

        internal float[] GetWds(int[] indices)
        {
            float[] wds = new float[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                int index = indices[i];
                if (ParamDict.ContainsKey(index))
                    wds[i] *= ParamDict[index].Wd_Mult;
                else if (wd_mult.ContainsKey(index.ToString()))
                    wds[i] *= wd_mult[index.ToString()];
                else if (Idx2Name.ContainsKey(index))
                {
                    float Idx2Name_lrvalue = 1;
                    if (float.TryParse(Idx2Name[index], out Idx2Name_lrvalue))
                    {
                        wds[i] *= Idx2Name_lrvalue;
                    }
                }
            }

            return wds;
        }

        internal float GetWd(int index) => GetWds(new int[] { index })[0];

        internal static NDArray[] FlattenList(params NDArray[][] nested_list)
        {
            List<NDArray> result = new List<NDArray>();
            foreach (var item in nested_list)
            {
                result.AddRange(item);
            }

            return result.ToArray();
        }

        public Updater GetUpdater()
        {
            return new Updater(this);
        }
    }
}