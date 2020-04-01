﻿/*****************************************************************************
   Copyright 2018 The MxNet.Sharp Authors. All Rights Reserved.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
******************************************************************************/
using System;
using NumpyDotNet;

namespace MxNet.Metrics
{
    public class CrossEntropy : EvalMetric
    {
        private readonly float eps;

        public CrossEntropy(float eps = 1e-12f, string output_name = null, string label_name = null) : base(
            "cross-entropy", output_name, label_name, true)
        {
            this.eps = eps;
        }

        public override void Update(NDArray labels, NDArray preds)
        {
            var l = labels.AsNumpy();
            if (preds.Shape[0] != labels.Shape[0])
                throw new ArgumentException("preds.Shape[0] != labels.Shape[0]");

            l = l.ravel();
            var p = preds.AsNumpy();
            var prob = p[np.arange(l.shape.iDims[0]), l.astype(np.Int64)];
            var cross_entropy = np.sum(-np.log((ndarray)prob + eps)).asscalar<float>();
            sum_metric += sum_metric;
            global_sum_metric += sum_metric;
            num_inst += (int)l.shape.iDims[0];
            global_num_inst += (int)l.shape.iDims[0];
        }
    }
}