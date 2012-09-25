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

using System;
using System.Collections.Generic;
using System.Configuration;
using ZeroG.Data.Database;
using ZeroG.Data.Object.Cache;
using ZeroG.Data.Object.Metadata;

namespace ZeroG.Data.Object.Index
{
    internal sealed class ObjectIndexer : IDisposable
    {
        private IObjectIndexProvider _indexer;
        private ObjectIndexerCache _cache;

        private static Type _indexerType = null;

        public ObjectIndexer(ObjectIndexerCache cache) : this()
        {
            _cache = cache;
        }

        public ObjectIndexer()
        {
            if (null == _indexerType)
            {
                var setting = ConfigurationManager.AppSettings[Config.ObjectIndexProviderConfigKey];
                if (!string.IsNullOrEmpty(setting))
                {
                    var objectIndexProviderType = Type.GetType(setting, true);

                    if (typeof(IObjectIndexProvider).IsAssignableFrom(objectIndexProviderType))
                    {
                        _indexer = (IObjectIndexProvider)Activator.CreateInstance(objectIndexProviderType);
                        _indexerType = objectIndexProviderType;
                    }
                    else
                    {
                        throw new InvalidOperationException("Unsupported IObjectIndexProvider type: " + objectIndexProviderType.FullName);
                    }
                }
            }
            else
            {
                _indexer = (IObjectIndexProvider)Activator.CreateInstance(_indexerType);
            }
        }

        private bool _ValidateIndexes(string nameSpace, PersistentObject obj)
        {
            foreach (var idx in obj.Indexes)
            {
                var dataType = idx.GetDataType();
                if (null == obj.Value)
                {
                    throw new ArgumentNullException("obj.Value", "Index values cannot be null.");
                }
                else if (ObjectIndexType.Unknown == dataType)
                {
                    throw new ArgumentException("ObjectIndexType is Unknown. Unable to store index value.");
                }
            }
            return true;
        }

        public bool Exists(string objectFullName)
        {
            return _indexer.Exists(objectFullName);
        }

        internal int[] Find(string objectFullName, ObjectFindLogic logic, ObjectFindOperator oper, ObjectIndex[] indexes)
        {
            int[] returnValue = null;

            if (null != _cache)
            {
                var parameters = new object[3 + ((null == indexes) ? 0 : indexes.Length)];
                parameters[0] = objectFullName;
                parameters[1] = logic;
                parameters[2] = oper;
                if (null != indexes)
                {
                    for (int i = 0; indexes.Length > i; i++)
                    {
                        parameters[i + 3] = indexes[i];
                    }
                }
                returnValue = _cache.Get(parameters);
                if (null == returnValue)
                {
                    returnValue = _indexer.Find(objectFullName, logic, oper, indexes);
                    _cache.Set(returnValue, parameters);
                }
            }
            else
            {
                returnValue = _indexer.Find(objectFullName, logic, oper, indexes);
            }
            return returnValue;
        }

        public int[] Find(string objectFullName, string constraint, ObjectIndexMetadata[] indexes)
        {
            int[] returnValue = null;

            if (null != _cache)
            {
                var parameters = new object[2 + ((null == indexes) ? 0 : indexes.Length)];
                parameters[0] = objectFullName;
                parameters[1] = constraint;
                if (null != indexes)
                {
                    for (int i = 0; indexes.Length > i; i++)
                    {
                        parameters[i + 3] = indexes[i];
                    }
                }
                returnValue = _cache.Get(parameters);
                if (null == returnValue)
                {
                    returnValue = _indexer.Find(objectFullName, constraint, indexes);
                    _cache.Set(returnValue, parameters);
                }
            }
            else
            {
                returnValue = _indexer.Find(objectFullName, constraint, indexes);
            }
            return returnValue;
        }

        public void IndexObject(string nameSpace, PersistentObject obj)
        {
            if (_ValidateIndexes(nameSpace, obj))
            {
                _indexer.UpsertIndexValues(ObjectNaming.CreateFullObjectName(nameSpace, obj.Name), obj.ID, obj.Indexes);
            }
        }

        public void RemoveObjectIndex(string objectFullName, int objectId)
        {
            _indexer.RemoveIndexValue(objectFullName, objectId);
        }

        public void RemoveObjectIndexes(string objectFullName, int[] objectIds)
        {
            _indexer.RemoveIndexValues(objectFullName, objectIds);
        }

        public void ProvisionIndex(ObjectMetadata metadata)
        {
            _indexer.ProvisionIndex(metadata);
        }

        public void UnprovisionIndex(string objectFullName)
        {
            _indexer.UnprovisionIndex(objectFullName);
        }

        public void Truncate(string objectFullName)
        {
            _indexer.Truncate(objectFullName);
        }

        #region Dispose implementation
        private bool _disposed;

        public void Dispose()
        {
            _Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void _Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }
                _disposed = true;
            }

        }
        #endregion
    }
}
