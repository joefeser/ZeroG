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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ZeroG.Data.Database
{
    public class DatabaseServiceConfiguration
    {
        public readonly string Name;
        public readonly string TypeName;
        public readonly string ConnectionString;
        public readonly ICollection<DatabaseServiceConfigurationProperty> Properties;

        public DatabaseServiceConfiguration(string name,
            string typeName,
            string connectionString,
            Dictionary<string, string> additionalProperties)
        {
            Name = name;
            TypeName = typeName;
            ConnectionString = connectionString;
            if (null != additionalProperties)
            {
                Properties = new ReadOnlyCollection<DatabaseServiceConfigurationProperty>(
                    additionalProperties.Select(kv => new DatabaseServiceConfigurationProperty(kv.Key, kv.Value)).ToList());
            }
            else
            {
                Properties = new ReadOnlyCollection<DatabaseServiceConfigurationProperty>(new List<DatabaseServiceConfigurationProperty>());
            }
        }
    }
}
