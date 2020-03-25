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
namespace MxNet.Image
{
    public class ForceResizeAug : Augmenter
    {
        public ForceResizeAug((int, int) size, ImgInterp interp = ImgInterp.Area_Based)
        {
            Size = size;
            Interp = interp;
        }

        public (int, int) Size { get; set; }

        public ImgInterp Interp { get; set; }

        public override NDArray Call(NDArray src)
        {
            var sizes = (src.Shape[0], src.Shape[1], Size.Item2, Size.Item1);
            return Img.ImResize(src, Size.Item1, Size.Item2, Img.GetInterpMethod(Interp, sizes));
        }
    }
}