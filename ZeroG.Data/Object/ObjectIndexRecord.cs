﻿#region License, Terms and Conditions
// Copyright (c) 2012 Jeremy Burman
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System.Data;
using System.Runtime.Serialization;

namespace ZeroG.Data.Object
{
    [DataContract]
    public sealed class ObjectIndexRecord
    {
        public ObjectIndexRecord(ObjectIndex[] values)
        {
            Values = values;
        }

        public static ObjectIndexRecord CreateFromDataRecord(IDataRecord record)
        {
            var len = record.FieldCount;
            
            var values = new ObjectIndex[len];
            for (int i = 0; len > i; i++)
            {
                values[i] = ObjectIndex.Create(record.GetName(i), record.GetValue(i));
            }

            return new ObjectIndexRecord(values);
        }

        [DataMember(Order=1)]
        public ObjectIndex[] Values;
    }
}