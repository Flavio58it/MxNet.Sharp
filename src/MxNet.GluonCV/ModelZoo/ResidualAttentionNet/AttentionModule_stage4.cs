﻿using MxNet.Gluon;
using System;
using System.Collections.Generic;
using System.Text;

namespace MxNet.GluonCV.ModelZoo.ResidualAttentionNet
{
    public class AttentionModule_stage4 : HybridBlock
    {
        public AttentionModule_stage4(int channels, (float, float, float)? scale = null, string norm_layer = "BatchNorm", FuncArgs norm_kwargs = null, string prefix = "", ParameterDict @params = null) : base(prefix, @params)
        {
            throw new NotImplementedException();
        }

        public override NDArrayOrSymbol HybridForward(NDArrayOrSymbol x, params NDArrayOrSymbol[] args)
        {
            throw new NotImplementedException();
        }
    }
}
