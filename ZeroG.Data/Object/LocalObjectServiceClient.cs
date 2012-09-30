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
using System.IO;
using System.IO.Compression;

namespace ZeroG.Data.Object
{
    public class LocalObjectServiceClient : IObjectServiceClient
    {
        private const int _CompressionBufferSize = 4096;

        private ObjectService _service;
        private string _nameSpace, _objectName;

        public LocalObjectServiceClient(ObjectService service, string nameSpace, string objectName)
        {
            if (null == service) { throw new ArgumentNullException("service"); }
            if (null == nameSpace) { throw new ArgumentNullException("nameSpace"); }
            if (null == objectName) { throw new ArgumentNullException("objectName"); }

            _service = service;
            _nameSpace = nameSpace;
            _objectName = objectName;
        }

        #region Private helpers
        private byte[] _Compress(byte[] value)
        {
            var buffer = new MemoryStream();
            using (var zipStream = new GZipStream(buffer, CompressionMode.Compress))
            {
                zipStream.Write(value, 0, value.Length);
            }
            return buffer.ToArray();
        }

        private byte[] _Decompress(byte[] value)
        {
            if (null != value)
            {
                var inputStream = new MemoryStream(value);
                var outputStream = new MemoryStream();
                var buffer = new byte[_CompressionBufferSize];

                using (var zipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                {
                    int numRead = 0;

                    while (0 != (numRead = zipStream.Read(buffer, 0, _CompressionBufferSize)))
                    {
                        outputStream.Write(buffer, 0, numRead);
                    }
                    return outputStream.ToArray();
                }
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region Public Store methods

        public ObjectID Store(byte[] value)
        {
            return Store(null, value);
        }

        public ObjectID Store(byte[] secondaryKey, byte[] value)
        {
            return _service.Store(_nameSpace,
                new PersistentObject()
                {
                    Name = _objectName,
                    SecondaryKey = secondaryKey,
                    Value = value
                });
        }

        public ObjectID StoreCompressed(byte[] value)
        {
            return Store(null, _Compress(value));
        }

        public ObjectID StoreCompressed(byte[] secondaryKey, byte[] value)
        {
            return Store(secondaryKey, _Compress(value));
        }

        #endregion

        #region Public Get methods

        public byte[] Get(int objectId)
        {
            return _service.Get(_nameSpace, _objectName, objectId);
        }

        public byte[] GetBySecondaryKey(byte[] secondaryKey)
        {
            return _service.GetBySecondaryKey(_nameSpace, _objectName, secondaryKey);
        }

        public byte[] GetCompressed(int objectId)
        {
            return _Decompress(_service.Get(_nameSpace, _objectName, objectId));
        }

        public byte[] GetCompressedBySecondaryKey(byte[] secondaryKey)
        {
            return _Decompress(_service.GetBySecondaryKey(_nameSpace, _objectName, secondaryKey));
        }

        #endregion

        #region Public Remove methods

        public void Remove(int objectId)
        {
            _service.Remove(_nameSpace, _objectName, objectId);
        }

        public void Remove(int[] objectIds)
        {
            _service.Remove(_nameSpace, _objectName, objectIds);
        }
        
        #endregion

        #region Public Find methods

        #endregion
    }
}