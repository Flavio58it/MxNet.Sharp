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
namespace MxNet.Initializers
{
    public class FusedRNN : Initializer
    {
        public FusedRNN(int num_hidden, int num_layers, string mode, bool bidirectional = false, float forget_bias = 1)
        {
            //ToDo: Depended on RNN Layer implementation
        }

        public override void InitWeight(string name, ref NDArray arr)
        {
            //ToDo: Depended on RNN Layer implementation
        }
    }
}