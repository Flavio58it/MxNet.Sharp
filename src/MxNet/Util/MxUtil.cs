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
using System.Collections.Generic;
using System.Linq;

namespace MxNet
{
    public class MxUtil
    {
        public static string EnumToString<TEnum>(TEnum? _enum, List<string> convert) where TEnum : struct, IConvertible
        {
            if (_enum.HasValue)
            {
                var v = _enum.Value as object;
                return convert[(int) v];
            }

            return null;
        }

        public static void ValidateParam(string name, string value, params string[] validValues)
        {
            if (!validValues.Contains(value))
            {
                var message = "Invalid value for " + name + ". Valid values are " + string.Join(", ", validValues);
                throw new Exception(message);
            }
        }

        public static IntPtr[] GetNDArrayHandles(NDArrayList list)
        {
            return list.Select(x => x.GetHandle()).ToArray();
        }

        public static List<T> Set<T>(List<T> keys)
        {
            return keys.Distinct().OrderBy(x => x).ToList();
        }

        public static (Shape, Shape) GetSliceNotation(string slice, Shape shape)
        {
            string[] split = slice.Split(',');
            int[] begin = new int[split.Length];
            int[] end = new int[split.Length];
            for (int i = 0; i < split.Length; i++)
            {
                begin[i] = 0;
                end[i] = shape[i];
                var range = split[i].Contains(":") ? split[i].Split(':') : null;
                if (range != null)
                {
                    begin[i] = !string.IsNullOrEmpty(range[0]) ? Convert.ToInt32(range[0].Trim()) : begin[i];
                    end[i] = !string.IsNullOrEmpty(range[1]) ? Convert.ToInt32(range[1].Trim()) : end[i];
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(split[i]))
                    {
                        begin[i] = Convert.ToInt32(split[i].Trim());
                        end[i] = begin[i] + 1;
                    }
                }
            }

            return (new Shape(begin), new Shape(end));
        }
    }
}