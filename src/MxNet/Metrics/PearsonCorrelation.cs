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
using NumpyDotNet;

namespace MxNet.Metrics
{
    public class PearsonCorrelation : EvalMetric
    {
        public PearsonCorrelation(string output_name = null, string label_name = null)
            : base("pearsonr", output_name, label_name, true)
        {
        }

        public override void Update(NDArray labels, NDArray preds)
        {
            CheckLabelShapes(labels, preds, true);

            ndarray pearson_corr = (ndarray)nd.Correlation(labels.Ravel(), preds.Ravel()).AsNumpy()[0, 1];
            sum_metric += pearson_corr.asscalar<float>();
            global_sum_metric += pearson_corr.asscalar<float>();
            num_inst += 1;
            global_num_inst += 1;
        }
    }
}