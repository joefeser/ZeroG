﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using ZeroG.Data.Database;
using ZeroG.Data.Database.Drivers.Object.Provider;
using ZeroG.Data.Object;
using ZeroG.Data.Object.Index;
using ZeroG.Data.Object.Metadata;
using ZeroG.Lang;
using ZeroG.Tests.Object;

namespace ZeroG.Tests.Data.Drivers
{
    /// <summary>
    /// Summary description for IndexProviderTest
    /// </summary>
    [TestClass]
    public class IndexProviderTest
    {
        public IndexProviderTest()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        internal static string NameSpace1 = "ZGTestNS";
        internal static string ObjectName1 = "ZGTestObj1";
        internal static string ObjectFullName1 = "ZGTestNS.ZGTestObj1";

        #region Additional test attributes
        private static IObjectIndexProvider _provider;
        internal static IObjectIndexProvider IndexProvider
        {
            get
            {
                if (null == _provider)
                {
                    _provider = ObjectTestHelper.CreateObjectIndexProvider();
                }
                return _provider;
            }
        }

        [TestInitialize()]
        public void TestInitialize() 
        {
            try
            {
                IndexProvider.UnprovisionIndex(ObjectFullName1);
            }
            catch
            {
            }
        }
        
        [TestCleanup()]
        public void TestCleanup() 
        {
            try
            {
                IndexProvider.UnprovisionIndex(ObjectFullName1);
            }
            catch
            {
            }
        }
        
        #endregion

        [TestMethod]
        public void ProvisionIndex()
        {
            var provider = IndexProvider;

            provider.ProvisionIndex(
                new ObjectMetadata(NameSpace1, ObjectName1,
                    new ObjectIndexMetadata[]
                    {
                        new ObjectIndexMetadata("TestCol1", ObjectIndexType.Integer)
                    }));

            // verify that we can lookup using the index
            int[] ids = provider.Find(ObjectFullName1,
                ObjectIndex.Create("TestCol1", 100));

            Assert.IsNotNull(ids);

            Assert.AreEqual(0, ids.Length);
        }

        [TestMethod]
        public void UnprovisionTest()
        {
            var provider = IndexProvider;

            try
            {
                provider.ProvisionIndex(
                    new ObjectMetadata(NameSpace1, ObjectName1,
                        new ObjectIndexMetadata[]
                    {
                        new ObjectIndexMetadata("TestCol1", ObjectIndexType.Integer)
                    }));

                // verify that we can lookup using the index
                int[] ids = provider.Find(ObjectFullName1,
                    ObjectIndex.Create("TestCol1", 100));

                Assert.IsNotNull(ids);

                Assert.AreEqual(0, ids.Length);

                provider.UnprovisionIndex(ObjectFullName1);
            }
            catch(Exception ex)
            {
                Assert.Fail("Unexpected exception: " + ex.ToString());
            }

            Assert.IsFalse(provider.ObjectExists(ObjectFullName1));

            try
            {
                // should throw
                provider.Find(ObjectFullName1,
                    ObjectIndex.Create("TestCol1", 100));

                Assert.IsTrue(false, "Expected exception to be thrown before reaching this line.");
            }
            catch (DbException)
            {
            }
        }

        [TestMethod]
        public void SimpleFind()
        {
            var provider = IndexProvider;

            provider.ProvisionIndex(
                new ObjectMetadata(NameSpace1, ObjectName1,
                    new ObjectIndexMetadata[]
                    {
                        new ObjectIndexMetadata("TestCol1", ObjectIndexType.Integer),
                        new ObjectIndexMetadata("TestCol2", ObjectIndexType.String, 15)
                    }));

            provider.UpsertIndexValues(ObjectFullName1, 1,
                ObjectIndex.Create("TestCol1", 100),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 2,
                ObjectIndex.Create("TestCol1", 105),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 3,
                ObjectIndex.Create("TestCol1", 500),
                ObjectIndex.Create("TestCol2", "B"));

            provider.UpsertIndexValues(ObjectFullName1, 4,
                ObjectIndex.Create("TestCol1", 500),
                ObjectIndex.Create("TestCol2", "C"));


            // test single constraint value that should return a single result
            int[] ids = provider.Find(ObjectFullName1,
                ObjectIndex.Create("TestCol1", 100));

            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1, ids[0]);

            // test two constraint values that should return a single result
            ids = provider.Find(ObjectFullName1,
                ObjectIndex.Create("TestCol1", 100),
                ObjectIndex.Create("TestCol2", "A"));

            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1, ids[0]);

            // test single constraint value that should return two results
            ids = provider.Find(ObjectFullName1,
                ObjectIndex.Create("TestCol1", 500));

            Assert.AreEqual(2, ids.Length);
            Assert.AreEqual(3, ids[0]);
            Assert.AreEqual(4, ids[1]);

            // test single constraint value that should return zero results
            ids = provider.Find(ObjectFullName1,
                ObjectIndex.Create("TestCol1", 105),
                ObjectIndex.Create("TestCol2", "B"));

            Assert.AreEqual(0, ids.Length);
        }

        [TestMethod]
        public void SimpleFindDataTypes()
        {
            var provider = IndexProvider;

            provider.ProvisionIndex(
                new ObjectMetadata(NameSpace1, ObjectName1,
                    new ObjectIndexMetadata[]
                    {
                        new ObjectIndexMetadata("IntCol", ObjectIndexType.Integer),
                        new ObjectIndexMetadata("TextCol", ObjectIndexType.String, 15),
                        new ObjectIndexMetadata("DecCol", ObjectIndexType.Decimal, 7, 2),
                        new ObjectIndexMetadata("DateTimeCol", ObjectIndexType.DateTime),
                        new ObjectIndexMetadata("BinCol", ObjectIndexType.Binary, 16)
                    }));

            Int32 testInt = 3447;
            String testStr = "Test Value";
            Decimal testDec = 156.12M;
            DateTime testDate = new DateTime(2011, 2, 14, 3, 10, 0);
            Guid testGuid = new Guid("76F5FB10BAEF4DE09578B3EB91FF6653");

            provider.UpsertIndexValues(ObjectFullName1,
                1000,
                ObjectIndex.Create("IntCol", testInt),
                ObjectIndex.Create("TextCol", testStr),
                ObjectIndex.Create("DecCol", testDec),
                ObjectIndex.Create("DateTimeCol", testDate),
                ObjectIndex.Create("BinCol", testGuid.ToByteArray()));

            provider.UpsertIndexValues(ObjectFullName1,
                1001,
                ObjectIndex.Create("IntCol", 500),
                ObjectIndex.Create("TextCol", "asdf"),
                ObjectIndex.Create("DecCol", new Decimal(5.4)),
                ObjectIndex.Create("DateTimeCol", DateTime.UtcNow),
                ObjectIndex.Create("BinCol", Guid.NewGuid().ToByteArray()));

            int[] ids = provider.Find(ObjectFullName1, ObjectIndex.Create("ID", 1000));
            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1000, ids[0]);

            ids = provider.Find(ObjectFullName1, ObjectIndex.Create("IntCol", testInt));
            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1000, ids[0]);

            ids = provider.Find(ObjectFullName1, ObjectIndex.Create("TextCol", testStr));
            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1000, ids[0]);

            ids = provider.Find(ObjectFullName1, ObjectIndex.Create("DecCol", testDec));
            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1000, ids[0]);

            ids = provider.Find(ObjectFullName1, ObjectIndex.Create("DateTimeCol", testDate));
            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1000, ids[0]);

            ids = provider.Find(ObjectFullName1, ObjectIndex.Create("BinCol", testGuid.ToByteArray()));
            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1000, ids[0]);
        }

        [TestMethod]
        public void SimpleFindWithOr()
        {
            var provider = IndexProvider;

            provider.ProvisionIndex(
                new ObjectMetadata(NameSpace1, ObjectName1,
                    new ObjectIndexMetadata[]
                    {
                        new ObjectIndexMetadata("TestCol1", ObjectIndexType.Integer),
                        new ObjectIndexMetadata("TestCol2", ObjectIndexType.String, 15)
                    }));

            provider.UpsertIndexValues(ObjectFullName1, 1,
                ObjectIndex.Create("TestCol1", 100),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 2,
                ObjectIndex.Create("TestCol1", 105),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 3,
                ObjectIndex.Create("TestCol1", 500),
                ObjectIndex.Create("TestCol2", "B"));

            provider.UpsertIndexValues(ObjectFullName1, 4,
                ObjectIndex.Create("TestCol1", 500),
                ObjectIndex.Create("TestCol2", "C"));


            // test two constraints on the same index that should return a two results
            int[] ids = provider.Find(ObjectFullName1, new ObjectFindOptions()
                {
                    Logic = ObjectFindLogic.Or,
                    Operator = ObjectFindOperator.Equals
                },
                ObjectIndex.Create("TestCol1", 100),
                ObjectIndex.Create("TestCol1", 105));

            Assert.IsNotNull(ids);
            Assert.AreEqual(2, ids.Length);
            Assert.AreEqual(1, ids[0]);
            Assert.AreEqual(2, ids[1]);

            // test two constraint on separate indexes that should return two results
            ids = provider.Find(ObjectFullName1, new ObjectFindOptions()
                {
                    Logic = ObjectFindLogic.Or,
                    Operator = ObjectFindOperator.Equals
                },
                ObjectIndex.Create("TestCol1", 105),
                ObjectIndex.Create("TestCol2", "C"));

            Assert.IsNotNull(ids);
            Assert.AreEqual(2, ids.Length);
            Assert.AreEqual(2, ids[0]);
            Assert.AreEqual(4, ids[1]);
        }

        [TestMethod]
        public void SimpleFindWithLike()
        {
            var provider = IndexProvider;

            provider.ProvisionIndex(
                new ObjectMetadata(NameSpace1, ObjectName1,
                    new ObjectIndexMetadata[]
                    {
                        new ObjectIndexMetadata("TestCol1", ObjectIndexType.Integer),
                        new ObjectIndexMetadata("TestCol2", ObjectIndexType.String, 15)
                    }));

            provider.UpsertIndexValues(ObjectFullName1, 1,
                ObjectIndex.Create("TestCol1", 100),
                ObjectIndex.Create("TestCol2", "AsDf"));

            provider.UpsertIndexValues(ObjectFullName1, 2,
                ObjectIndex.Create("TestCol1", 105),
                ObjectIndex.Create("TestCol2", "ASdZZz"));

            provider.UpsertIndexValues(ObjectFullName1, 3,
                ObjectIndex.Create("TestCol1", 500),
                ObjectIndex.Create("TestCol2", "B"));

            // should return one result
            int[] ids = provider.Find(ObjectFullName1, new ObjectFindOptions()
                {
                    Logic = ObjectFindLogic.And,
                    Operator = ObjectFindOperator.Like
                },
                ObjectIndex.Create("TestCol1", 100));

            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1, ids[0]);

            // should return one result
            ids = provider.Find(ObjectFullName1, new ObjectFindOptions()
                {
                    Logic = ObjectFindLogic.Or,
                    Operator = ObjectFindOperator.Like
                },
                ObjectIndex.Create("TestCol2", "asdf"));

            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1, ids[0]);

            // should return two results
            ids = provider.Find(ObjectFullName1, new ObjectFindOptions()
                {
                    Logic = ObjectFindLogic.Or,
                    Operator = ObjectFindOperator.Like
                },
                ObjectIndex.Create("TestCol2", "as%"));

            Assert.IsNotNull(ids);
            Assert.AreEqual(2, ids.Length);
            Assert.AreEqual(1, ids[0]);
            Assert.AreEqual(2, ids[1]);
        }

        [TestMethod]
        public void JSONConstraintFind()
        {
            var provider = IndexProvider;

            var indexMetadata = new ObjectIndexMetadata[]
            {
                new ObjectIndexMetadata("TestCol1", ObjectIndexType.Integer),
                new ObjectIndexMetadata("TestCol2", ObjectIndexType.String, 15)
            };

            provider.ProvisionIndex(
                new ObjectMetadata(NameSpace1, ObjectName1,
                    indexMetadata));

            provider.UpsertIndexValues(ObjectFullName1, 1,
                ObjectIndex.Create("TestCol1", 100),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 2,
                ObjectIndex.Create("TestCol1", 105),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 3,
                ObjectIndex.Create("TestCol1", 500),
                ObjectIndex.Create("TestCol2", "B"));

            provider.UpsertIndexValues(ObjectFullName1, 4,
                ObjectIndex.Create("TestCol1", 500),
                ObjectIndex.Create("TestCol2", "C"));

            // test single constraint value that should return a single result
            int[] ids = provider.Find(ObjectFullName1,
                @"{ ""TestCol1"" : 100, ""Op"" : ""="" }",
                indexMetadata);

            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1, ids[0]);

            // test two constraint values that should return a single result
            ids = provider.Find(ObjectFullName1,
                @"{ ""TestCol1"" : 100, ""Op"" : ""="",
""AND"" : { ""TestCol2"" : ""A"", ""Op"" : ""=""}}",
                indexMetadata);

            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1, ids[0]);

            // test single constraint value that should return two results
            ids = provider.Find(ObjectFullName1,
                @"{ ""TestCol1"" : 500, ""Op"" : ""="" }",
                indexMetadata);

            Assert.AreEqual(2, ids.Length);
            Assert.AreEqual(3, ids[0]);
            Assert.AreEqual(4, ids[1]);

            // test single constraint value that should return zero results
            ids = provider.Find(ObjectFullName1,
                @"{ ""TestCol1"" : 105, ""Op"" : ""="",
""AND"" : { ""TestCol2"" : ""B"", ""Op"" : ""=""}}",
                indexMetadata);

            Assert.AreEqual(0, ids.Length);
        }

        [TestMethod]
        public void JSONConstraintOperators()
        {
            var provider = IndexProvider;

            var indexMetadata = new ObjectIndexMetadata[]
            {
                new ObjectIndexMetadata("TestCol1", ObjectIndexType.Integer),
                new ObjectIndexMetadata("TestCol2", ObjectIndexType.String, 15)
            };

            provider.ProvisionIndex(
                new ObjectMetadata(NameSpace1, ObjectName1,
                    indexMetadata));

            provider.UpsertIndexValues(ObjectFullName1, 1,
                ObjectIndex.Create("TestCol1", 100),
                ObjectIndex.Create("TestCol2", "asdf"));

            provider.UpsertIndexValues(ObjectFullName1, 2,
                ObjectIndex.Create("TestCol1", 200),
                ObjectIndex.Create("TestCol2", "zxzy"));

            // test LIKE operator
            int[] ids = provider.Find(ObjectFullName1,
                @"{ ""TestCol2"" : ""%sd%"", ""Op"" : ""LIKE"" }",
                indexMetadata);

            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1, ids[0]);

            // test NOT LIKE operator
            ids = provider.Find(ObjectFullName1,
                @"{ ""TestCol2"" : ""as%"", ""Op"" : ""NOT LIKE"" }",
                indexMetadata);

            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(2, ids[0]);

            // test EQUALS operator
            ids = provider.Find(ObjectFullName1,
                @"{ ""TestCol1"" : 100, ""Op"" : ""="" }",
                indexMetadata);

            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1, ids[0]);

            // test NOT EQUALS operator
            ids = provider.Find(ObjectFullName1,
                @"{ ""TestCol1"" : 100, ""Op"" : ""<>"" }",
                indexMetadata);

            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(2, ids[0]);

            // test IN operator
            ids = provider.Find(ObjectFullName1,
                @"{ ""TestCol1"" : [100], ""Op"" : ""IN"" }",
                indexMetadata);

            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1, ids[0]);

            // test NOT IN operator
            ids = provider.Find(ObjectFullName1,
                @"{ ""TestCol1"" : [100], ""Op"" : ""NOT IN"" }",
                indexMetadata);

            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(2, ids[0]);

            // test LESS THAN operator
            ids = provider.Find(ObjectFullName1,
                @"{ ""TestCol1"" : 200, ""Op"" : ""<"" }",
                indexMetadata);

            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1, ids[0]);

            // test LESS THAN OR EQUALS operator
            ids = provider.Find(ObjectFullName1,
                @"{ ""TestCol1"" : 100, ""Op"" : ""<="" }",
                indexMetadata);

            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1, ids[0]);

            // test GREATER THAN operator
            ids = provider.Find(ObjectFullName1,
                @"{ ""TestCol1"" : 100, ""Op"" : "">"" }",
                indexMetadata);

            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(2, ids[0]);

            // test GREATER THAN OR EQUALS operator
            ids = provider.Find(ObjectFullName1,
                @"{ ""TestCol1"" : 200, ""Op"" : "">="" }",
                indexMetadata);

            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(2, ids[0]);
        }

        [TestMethod]
        public void Exists()
        {
            var provider = IndexProvider;
            var indexMetadata = new ObjectIndexMetadata[]
            {
                new ObjectIndexMetadata("TestCol1", ObjectIndexType.Integer),
                new ObjectIndexMetadata("TestCol2", ObjectIndexType.String, 15)
            };

            provider.ProvisionIndex(
                new ObjectMetadata(NameSpace1, ObjectName1,
                    indexMetadata));

            provider.UpsertIndexValues(ObjectFullName1, 1,
                ObjectIndex.Create("TestCol1", 100),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 2,
                ObjectIndex.Create("TestCol1", 105),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 3,
                ObjectIndex.Create("TestCol1", 500),
                ObjectIndex.Create("TestCol2", "B"));

            provider.UpsertIndexValues(ObjectFullName1, 4,
                ObjectIndex.Create("TestCol1", 500),
                ObjectIndex.Create("TestCol2", "C"));


            Assert.IsTrue(provider.Exists(ObjectFullName1,
                @"{ ""TestCol1"" : 100, ""Op"" : ""="" }", indexMetadata));

            Assert.IsFalse(provider.Exists(ObjectFullName1,
                @"{ ""TestCol1"" : 102, ""Op"" : ""="" }", indexMetadata));
        }

        [TestMethod]
        public void CountByFindOptions()
        {
            var provider = IndexProvider;
            var indexMetadata = new ObjectIndexMetadata[]
            {
                new ObjectIndexMetadata("TestCol1", ObjectIndexType.Integer),
                new ObjectIndexMetadata("TestCol2", ObjectIndexType.String, 15, true)
            };

            provider.ProvisionIndex(
                new ObjectMetadata(NameSpace1, ObjectName1,
                    indexMetadata));

            provider.UpsertIndexValues(ObjectFullName1, 1,
                ObjectIndex.Create("TestCol1", 100),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 2,
                ObjectIndex.Create("TestCol1", 105),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 3,
                ObjectIndex.Create("TestCol1", 500),
                ObjectIndex.Create("TestCol2", "B"));

            provider.UpsertIndexValues(ObjectFullName1, 4,
                ObjectIndex.Create("TestCol1", 500),
                ObjectIndex.Create("TestCol2", null));

            Assert.AreEqual(1, provider.Count(ObjectFullName1,
                new ObjectFindOptions() 
                {
                    Operator = ObjectFindOperator.Equals
                },
                new ObjectIndex[] { ObjectIndex.Create("TestCol1", 100) }));

            Assert.AreEqual(0, provider.Count(ObjectFullName1,
                new ObjectFindOptions()
                {
                    Operator = ObjectFindOperator.Equals
                },
                new ObjectIndex[] { ObjectIndex.Create("TestCol1", 102) }));

            Assert.AreEqual(1, provider.Count(ObjectFullName1,
                new ObjectFindOptions()
                {
                    Operator = ObjectFindOperator.IsNull
                },
                new ObjectIndex[] { ObjectIndex.Create("TestCol2", null) }));

            Assert.AreEqual(2, provider.Count(ObjectFullName1,
                new ObjectFindOptions()
                {
                    Operator = ObjectFindOperator.Like
                },
                new ObjectIndex[] { ObjectIndex.Create("TestCol2", "a") }));

            Assert.AreEqual(3, provider.Count(ObjectFullName1,
                new ObjectFindOptions()
                {
                    Operator = ObjectFindOperator.Like,
                    Logic = ObjectFindLogic.Or
                },
                new ObjectIndex[] 
                { 
                    ObjectIndex.Create("TestCol2", "a"),
                    ObjectIndex.Create("TestCol2", "B")
                }));            
        }

        [TestMethod]
        public void CountByConstraint()
        {
            var provider = IndexProvider;
            var indexMetadata = new ObjectIndexMetadata[]
            {
                new ObjectIndexMetadata("TestCol1", ObjectIndexType.Integer),
                new ObjectIndexMetadata("TestCol2", ObjectIndexType.String, 15)
            };

            provider.ProvisionIndex(
                new ObjectMetadata(NameSpace1, ObjectName1,
                    indexMetadata));

            provider.UpsertIndexValues(ObjectFullName1, 1,
                ObjectIndex.Create("TestCol1", 100),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 2,
                ObjectIndex.Create("TestCol1", 105),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 3,
                ObjectIndex.Create("TestCol1", 500),
                ObjectIndex.Create("TestCol2", "B"));

            provider.UpsertIndexValues(ObjectFullName1, 4,
                ObjectIndex.Create("TestCol1", 500),
                ObjectIndex.Create("TestCol2", "C"));


            Assert.AreEqual(1, provider.Count(ObjectFullName1,
                @"{ ""TestCol1"" : 100, ""Op"" : ""="" }", indexMetadata));

            Assert.AreEqual(0, provider.Count(ObjectFullName1,
                @"{ ""TestCol1"" : 102, ""Op"" : ""="" }", indexMetadata));

            Assert.AreEqual(4, provider.Count(ObjectFullName1,
                @"{ ""TestCol1"" : 0, ""Op"" : "">"" }", indexMetadata));

            Assert.AreEqual(2, provider.Count(ObjectFullName1,
                @"{ ""TestCol2"" : ""a"", ""Op"" : ""LIKE"" }", indexMetadata));
        }

        [TestMethod]
        public void Iterate()
        {
            var provider = IndexProvider;
            var indexMetadata = new ObjectIndexMetadata[]
            {
                new ObjectIndexMetadata("TestCol1", ObjectIndexType.Integer),
                new ObjectIndexMetadata("TestCol2", ObjectIndexType.String, 15)
            };

            provider.ProvisionIndex(
                new ObjectMetadata(NameSpace1, ObjectName1,
                    indexMetadata));

            provider.UpsertIndexValues(ObjectFullName1, 1,
                ObjectIndex.Create("TestCol1", 100),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 2,
                ObjectIndex.Create("TestCol1", 105),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 3,
                ObjectIndex.Create("TestCol1", 500),
                ObjectIndex.Create("TestCol2", "B"));

            provider.UpsertIndexValues(ObjectFullName1, 4,
                ObjectIndex.Create("TestCol1", 500),
                ObjectIndex.Create("TestCol2", "C"));

            IEnumerator<IDataRecord> records;

            using (records = provider.Iterate(ObjectFullName1,
                null, 0, null, null, indexMetadata).GetEnumerator())
            {
                if (records.MoveNext())
                {
                    Assert.AreEqual(3, records.Current.FieldCount);

                    // ID
                    Assert.AreEqual(1, records.Current[0]);
                    Assert.AreEqual(1, records.Current["ID"]);
                    // TestCol1
                    Assert.AreEqual(100, records.Current[1]);
                    Assert.AreEqual(100, records.Current["TestCol1"]);
                    // TestCol2
                    Assert.AreEqual("A", records.Current[2]);
                    Assert.AreEqual("A", records.Current["TestCol2"]);
                }

                if (records.MoveNext())
                {
                    // ID
                    Assert.AreEqual(2, records.Current[0]);
                    Assert.AreEqual(2, records.Current["ID"]);
                    // TestCol1
                    Assert.AreEqual(105, records.Current[1]);
                    Assert.AreEqual(105, records.Current["TestCol1"]);
                    // TestCol2
                    Assert.AreEqual("A", records.Current[2]);
                    Assert.AreEqual("A", records.Current["TestCol2"]);
                }
            }

            using (records = provider.Iterate(ObjectFullName1, indexMetadata).GetEnumerator())
            {
                if (records.MoveNext())
                {
                    Assert.AreEqual(3, records.Current.FieldCount);

                    // ID
                    Assert.AreEqual(1, records.Current[0]);
                    Assert.AreEqual(1, records.Current["ID"]);
                    // TestCol1
                    Assert.AreEqual(100, records.Current[1]);
                    Assert.AreEqual(100, records.Current["TestCol1"]);
                    // TestCol2
                    Assert.AreEqual("A", records.Current[2]);
                    Assert.AreEqual("A", records.Current["TestCol2"]);
                }

                if (records.MoveNext())
                {
                    // ID
                    Assert.AreEqual(2, records.Current[0]);
                    Assert.AreEqual(2, records.Current["ID"]);
                    // TestCol1
                    Assert.AreEqual(105, records.Current[1]);
                    Assert.AreEqual(105, records.Current["TestCol1"]);
                    // TestCol2
                    Assert.AreEqual("A", records.Current[2]);
                    Assert.AreEqual("A", records.Current["TestCol2"]);
                }
            }

            // test with constraint, limit, and order
            using (records = provider.Iterate(ObjectFullName1,
                 @"{ ""TestCol2"" : ""b"", ""Op"" : ""LIKE"", ""OR"" : { ""TestCol2"" : ""c"", ""Op"" : ""LIKE"" } }", 2, new OrderOptions()
                {
                    Descending = true,
                    Indexes = new string[] { "TestCol2" }
                }, null, indexMetadata).GetEnumerator())
            {
                if (records.MoveNext())
                {
                    Assert.AreEqual(3, records.Current.FieldCount);

                    // ID
                    Assert.AreEqual(4, records.Current[0]);
                    Assert.AreEqual(4, records.Current["ID"]);
                    // TestCol1
                    Assert.AreEqual(500, records.Current[1]);
                    Assert.AreEqual(500, records.Current["TestCol1"]);
                    // TestCol2
                    Assert.AreEqual("C", records.Current[2]);
                    Assert.AreEqual("C", records.Current["TestCol2"]);
                }

                if (records.MoveNext())
                {
                    // ID
                    Assert.AreEqual(3, records.Current[0]);
                    Assert.AreEqual(3, records.Current["ID"]);
                    // TestCol1
                    Assert.AreEqual(500, records.Current[1]);
                    Assert.AreEqual(500, records.Current["TestCol1"]);
                    // TestCol2
                    Assert.AreEqual("B", records.Current[2]);
                    Assert.AreEqual("B", records.Current["TestCol2"]);
                }
            }

            // iterate only one index
            using (records = provider.Iterate(ObjectFullName1,
                 @"{ ""TestCol2"" : ""b"", ""Op"" : ""LIKE"", ""OR"" : { ""TestCol1"" : 105, ""Op"" : ""="" } }", 
                 0, null, new string[] { "TestCol1" }, indexMetadata).GetEnumerator())
            {
                if (records.MoveNext())
                {
                    Assert.AreEqual(1, records.Current.FieldCount);

                    // TestCol1
                    Assert.AreEqual(105, records.Current[0]);
                    Assert.AreEqual(105, records.Current["TestCol1"]);
                }

                if (records.MoveNext())
                {
                    // TestCol1
                    Assert.AreEqual(500, records.Current[0]);
                    Assert.AreEqual(500, records.Current["TestCol1"]);
                }
            }

            // iterate only two indexes in order
            using (records = provider.Iterate(ObjectFullName1,
                 @"{ ""TestCol2"" : ""b"", ""Op"" : ""LIKE"", ""OR"" : { ""TestCol2"" : ""c"", ""Op"" : ""LIKE"" } }", 2, new OrderOptions()
                 {
                     Descending = false,
                     Indexes = new string[] { "TestCol2" }
                 }, new string[] { "ID", "TestCol1" }, indexMetadata).GetEnumerator())
            {
                if (records.MoveNext())
                {
                    Assert.AreEqual(2, records.Current.FieldCount);
                    
                    // ID
                    Assert.AreEqual(3, records.Current[0]);
                    Assert.AreEqual(3, records.Current["ID"]);
                    // TestCol1
                    Assert.AreEqual(500, records.Current[1]);
                    Assert.AreEqual(500, records.Current["TestCol1"]);
                }

                if (records.MoveNext())
                {
                    // ID
                    Assert.AreEqual(4, records.Current[0]);
                    Assert.AreEqual(4, records.Current["ID"]);
                    // TestCol1
                    Assert.AreEqual(500, records.Current[1]);
                    Assert.AreEqual(500, records.Current["TestCol1"]);
                }
            }
        }

        [TestMethod]
        public void Limit()
        {
            var provider = IndexProvider;
            var indexMetadata = new ObjectIndexMetadata[]
            {
                new ObjectIndexMetadata("TestCol1", ObjectIndexType.Integer),
                new ObjectIndexMetadata("TestCol2", ObjectIndexType.String, 15)
            };

            provider.ProvisionIndex(
                new ObjectMetadata(NameSpace1, ObjectName1,
                    indexMetadata));

            provider.UpsertIndexValues(ObjectFullName1, 1,
                ObjectIndex.Create("TestCol1", 100),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 2,
                ObjectIndex.Create("TestCol1", 105),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 3,
                ObjectIndex.Create("TestCol1", 500),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 4,
                ObjectIndex.Create("TestCol1", 500),
                ObjectIndex.Create("TestCol2", "B"));

            var ids = provider.Find(ObjectFullName1,
                new ObjectFindOptions()
                {
                    Logic = ObjectFindLogic.And,
                    Operator = ObjectFindOperator.Equals,
                    Limit = 0
                }, ObjectIndex.Create("TestCol2", "A"));

            Assert.AreEqual(3, ids.Length);

            ids = provider.Find(ObjectFullName1,
                new ObjectFindOptions()
                {
                    Logic = ObjectFindLogic.And,
                    Operator = ObjectFindOperator.Equals,
                    Limit = 1
                }, ObjectIndex.Create("TestCol2", "A"));

            Assert.AreEqual(1, ids.Length);

            ids = provider.Find(ObjectFullName1,
                new ObjectFindOptions()
                {
                    Logic = ObjectFindLogic.And,
                    Operator = ObjectFindOperator.Equals,
                    Limit = 2
                }, ObjectIndex.Create("TestCol2", "A"));

            Assert.AreEqual(2, ids.Length);

            ids = provider.Find(ObjectFullName1,
                new ObjectFindOptions()
                {
                    Logic = ObjectFindLogic.And,
                    Operator = ObjectFindOperator.Equals,
                    Limit = 10
                }, ObjectIndex.Create("TestCol2", "A"));

            Assert.AreEqual(3, ids.Length);

            ids = provider.Find(ObjectFullName1,
                @"{ ""TestCol2"" : ""a"", ""Op"" : ""LIKE"" }", 0, null, indexMetadata);
            Assert.AreEqual(3, ids.Length);

            ids = provider.Find(ObjectFullName1,
                @"{ ""TestCol2"" : ""a"", ""Op"" : ""LIKE"" }", 1, null, indexMetadata);
            Assert.AreEqual(1, ids.Length);

            ids = provider.Find(ObjectFullName1,
                @"{ ""TestCol2"" : ""a"", ""Op"" : ""LIKE"" }", 2, null, indexMetadata);
            Assert.AreEqual(2, ids.Length);

            ids = provider.Find(ObjectFullName1,
                @"{ ""TestCol2"" : ""a"", ""Op"" : ""LIKE"" }", 10, null, indexMetadata);
            Assert.AreEqual(3, ids.Length);
        }

        [TestMethod]
        public void Order()
        {
            var provider = IndexProvider;
            var indexMetadata = new ObjectIndexMetadata[]
            {
                new ObjectIndexMetadata("TestCol1", ObjectIndexType.Integer),
                new ObjectIndexMetadata("TestCol2", ObjectIndexType.String, 15)
            };

            provider.ProvisionIndex(
                new ObjectMetadata(NameSpace1, ObjectName1,
                    indexMetadata));

            provider.UpsertIndexValues(ObjectFullName1, 1,
                ObjectIndex.Create("TestCol1", 100),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 2,
                ObjectIndex.Create("TestCol1", 105),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 3,
                ObjectIndex.Create("TestCol1", 500),
                ObjectIndex.Create("TestCol2", "B"));

            provider.UpsertIndexValues(ObjectFullName1, 4,
                ObjectIndex.Create("TestCol1", 500),
                ObjectIndex.Create("TestCol2", "C"));

            var vals = provider.Find(ObjectFullName1,
                @"{ ""TestCol2"" : ""A"", ""Op"" : ""="" }",
                0,
                new OrderOptions()
                {
                    Descending = true,
                    Indexes = new string[] { "TestCol1" }
                },
                indexMetadata);

            Assert.AreEqual(2, vals.Length);
            Assert.AreEqual(2, vals[0]);
            Assert.AreEqual(1, vals[1]);

            vals = provider.Find(ObjectFullName1,
                @"{ ""TestCol2"" : ""A"", ""Op"" : ""="" }",
                0,
                new OrderOptions()
                {
                    Descending = false,
                    Indexes = new string[] { "TestCol1" }
                },
                indexMetadata);

            Assert.AreEqual(2, vals.Length);
            Assert.AreEqual(1, vals[0]);
            Assert.AreEqual(2, vals[1]);

            vals = provider.Find(ObjectFullName1,
                new ObjectFindOptions() 
                {
                    Logic = ObjectFindLogic.And,
                    Operator = ObjectFindOperator.Equals,
                    Limit = 0,
                    Order = new OrderOptions()
                    {
                        Descending = true,
                        Indexes = new string[] { "TestCol1" }
                    }
                },
                ObjectIndex.Create("TestCol2", "A"));

            Assert.AreEqual(2, vals.Length);
            Assert.AreEqual(2, vals[0]);
            Assert.AreEqual(1, vals[1]);

            vals = provider.Find(ObjectFullName1,
                new ObjectFindOptions()
                {
                    Logic = ObjectFindLogic.And,
                    Operator = ObjectFindOperator.Equals,
                    Limit = 0,
                    Order = new OrderOptions()
                    {
                        Descending = false,
                        Indexes = new string[] { "TestCol1" }
                    }
                },
                ObjectIndex.Create("TestCol2", "A"));

            Assert.AreEqual(2, vals.Length);
            Assert.AreEqual(1, vals[0]);
            Assert.AreEqual(2, vals[1]);

            // Test and Order with Limit
            vals = provider.Find(ObjectFullName1,
                new ObjectFindOptions()
                {
                    Logic = ObjectFindLogic.And,
                    Operator = ObjectFindOperator.Equals,
                    Limit = 1,
                    Order = new OrderOptions()
                    {
                        Descending = true,
                        Indexes = new string[] { "TestCol1" }
                    }
                },
                ObjectIndex.Create("TestCol2", "A"));

            Assert.AreEqual(1, vals.Length);
            Assert.AreEqual(2, vals[0]);

            vals = provider.Find(ObjectFullName1,
                new ObjectFindOptions()
                {
                    Logic = ObjectFindLogic.And,
                    Operator = ObjectFindOperator.Equals,
                    Limit = 1,
                    Order = new OrderOptions()
                    {
                        Descending = false,
                        Indexes = new string[] { "TestCol1" }
                    }
                },
                ObjectIndex.Create("TestCol2", "A"));

            Assert.AreEqual(1, vals.Length);
            Assert.AreEqual(1, vals[0]);
        }


        [TestMethod]
        public void BulkUpsert()
        {
            var provider = IndexProvider;
            var metadata = new ObjectMetadata(NameSpace1, ObjectName1,
                    new ObjectIndexMetadata[]
                    {
                        new ObjectIndexMetadata("IntCol", ObjectIndexType.Integer),
                        new ObjectIndexMetadata("TextCol", ObjectIndexType.String, 15),
                        new ObjectIndexMetadata("DecCol", ObjectIndexType.Decimal, 7, 2),
                        new ObjectIndexMetadata("DateTimeCol", ObjectIndexType.DateTime),
                        new ObjectIndexMetadata("BinCol", ObjectIndexType.Binary, 16)
                    });

            provider.ProvisionIndex(
                metadata);

            Int32 testInt = 3447;
            String testStr = "Test Value";
            Decimal testDec = 156.12M;
            DateTime testDate = new DateTime(2011, 2, 14, 3, 10, 0);
            Guid testGuid = new Guid("76F5FB10BAEF4DE09578B3EB91FF6653");

            provider.BulkUpsertIndexValues(
                ObjectFullName1, 
                metadata,
                new object[][]
                {
                    new object[] { 1000, testInt, testStr, testDec, testDate, testGuid.ToByteArray() },
                    new object[] { 500, 0, "asdf", new Decimal(5.4), DateTime.UtcNow, Guid.NewGuid().ToByteArray() }
                });

            Assert.AreEqual(2, provider.CountObjects(ObjectFullName1));

            int[] ids = provider.Find(ObjectFullName1, ObjectIndex.Create("ID", 1000));
            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1000, ids[0]);

            ids = provider.Find(ObjectFullName1, ObjectIndex.Create("IntCol", testInt));
            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1000, ids[0]);

            ids = provider.Find(ObjectFullName1, ObjectIndex.Create("TextCol", testStr));
            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1000, ids[0]);

            ids = provider.Find(ObjectFullName1, ObjectIndex.Create("DecCol", testDec));
            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1000, ids[0]);

            ids = provider.Find(ObjectFullName1, ObjectIndex.Create("DateTimeCol", testDate));
            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1000, ids[0]);

            ids = provider.Find(ObjectFullName1, ObjectIndex.Create("BinCol", testGuid.ToByteArray()));
            Assert.IsNotNull(ids);
            Assert.AreEqual(1, ids.Length);
            Assert.AreEqual(1000, ids[0]);

            // test that values are updated
            provider.BulkUpsertIndexValues(
                ObjectFullName1,
                metadata,
                new object[][]
                {
                    new object[] { 1000, testInt, testStr, testDec, testDate, testGuid.ToByteArray() },
                    new object[] { 500, 5, "asdfupdated", new Decimal(5.7), DateTime.UtcNow, Guid.NewGuid().ToByteArray() }
                });

            Assert.AreEqual(2, provider.CountObjects(ObjectFullName1));
            Assert.AreEqual(0, provider.Count(ObjectFullName1, @"{""IntCol"" : 0}", metadata.Indexes));
            Assert.AreEqual(1, provider.Count(ObjectFullName1, @"{""IntCol"" : 5}", metadata.Indexes));
            Assert.AreEqual(0, provider.Count(ObjectFullName1, @"{""TextCol"" : ""asdf""}", metadata.Indexes));
            Assert.AreEqual(1, provider.Count(ObjectFullName1, @"{""TextCol"" : ""asdfupdated""}", metadata.Indexes));
        }

        [TestMethod]
        public void TopByFind()
        {
            var provider = IndexProvider;
            var indexMetadata = new ObjectIndexMetadata[]
            {
                new ObjectIndexMetadata("TestCol1", ObjectIndexType.Integer),
                new ObjectIndexMetadata("TestCol2", ObjectIndexType.String, 15, true)
            };

            provider.ProvisionIndex(
                new ObjectMetadata(NameSpace1, ObjectName1,
                    indexMetadata));

            provider.UpsertIndexValues(ObjectFullName1, 1,
                ObjectIndex.Create("TestCol1", 100),
                ObjectIndex.Create("TestCol2", "A"));

            provider.UpsertIndexValues(ObjectFullName1, 2,
                ObjectIndex.Create("TestCol1", 105),
                ObjectIndex.Create("TestCol2", null));

            var result = provider.Find(ObjectFullName1, new ObjectFindOptions()
            {
                Limit = 1,
                Order = new OrderOptions()
                {
                    Descending = true,
                    Indexes = new string[] { "TestCol1" }
                }
            });

            Assert.AreEqual(1, result.Length);

            Assert.AreEqual(2, result[0]);
        }
    }
}
