using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections.Generic;
using ProjectChimera.Core;
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Testing.Data
{
    /// <summary>
    /// Test suite for Unicode JSON handling fixes.
    /// Validates that invalid surrogate pairs are properly handled during JSON serialization.
    /// </summary>
    [TestFixture]
    [Category("Unicode JSON")]
    public class UnicodeJsonTests
    {
        #region Test Data Setup

        private class TestDataWithUnicode
        {
            public string Name;
            public string Description;
            public List<string> Tags = new List<string>();
            public Dictionary<string, string> Properties = new Dictionary<string, string>();
        }

        #endregion

        #region Unicode Detection Tests

        [Test]
        public void Test_UnicodeDetection_ValidText()
        {
            // Arrange
            string validText = "Normal text with Unicode: 🌱🍃";
            
            // Act
            bool hasIssues = UnicodeDataDiagnostic.HasUnicodeIssues(validText, out var issues);
            
            // Assert
            Assert.IsFalse(hasIssues, "Valid Unicode text should not have issues");
            Assert.AreEqual(0, issues.Count, "Should have no Unicode issues");
        }

        [Test]
        public void Test_UnicodeDetection_InvalidHighSurrogate()
        {
            // Arrange - High surrogate without low surrogate (construct programmatically)
            string invalidText = "Test" + (char)0xD800 + "Invalid";
            
            // Act
            bool hasIssues = UnicodeDataDiagnostic.HasUnicodeIssues(invalidText, out var issues);
            
            // Assert
            Assert.IsTrue(hasIssues, "Text with invalid high surrogate should have issues");
            Assert.AreEqual(1, issues.Count, "Should detect one Unicode issue");
            Assert.AreEqual("Invalid high surrogate", issues[0].IssueType);
        }

        [Test]
        public void Test_UnicodeDetection_OrphanedLowSurrogate()
        {
            // Arrange - Low surrogate without high surrogate (construct programmatically)
            string invalidText = "Test" + (char)0xDC00 + "Orphaned";
            
            // Act
            bool hasIssues = UnicodeDataDiagnostic.HasUnicodeIssues(invalidText, out var issues);
            
            // Assert
            Assert.IsTrue(hasIssues, "Text with orphaned low surrogate should have issues");
            Assert.AreEqual(1, issues.Count, "Should detect one Unicode issue");
            Assert.AreEqual("Orphaned low surrogate", issues[0].IssueType);
        }

        #endregion

        #region JSON Helper Tests

        [Test]
        public void Test_JsonHelper_SafeSerialization()
        {
            // Arrange
            var testData = new TestDataWithUnicode
            {
                Name = "Test" + (char)0xD800 + "Invalid", // Invalid Unicode
                Description = "Valid description",
                Tags = new List<string> { "Tag1", "Test" + (char)0xDC00 + "Orphaned" }, // Invalid Unicode in list
                Properties = new Dictionary<string, string>
                {
                    { "Key1", "Valid value" },
                    { "Key2", "Test" + (char)0xD800 + (char)0xDC00 + "Valid" } // Valid surrogate pair
                }
            };
            
            // Act
            string json = JsonUtilityHelper.ToJsonSafe(testData, true);
            
            // Assert
            Assert.IsNotEmpty(json, "Should produce valid JSON");
            Assert.IsTrue(JsonUtilityHelper.IsValidJson(json), "JSON should be valid");
            
            // Verify no invalid surrogates remain
            Assert.IsFalse(UnicodeDataDiagnostic.HasUnicodeIssues(json, out var issues), 
                $"Serialized JSON should not have Unicode issues. Found: {issues?.Count ?? 0}");
        }

        [Test]
        public void Test_JsonHelper_SafeDeserialization()
        {
            // Arrange - Use escaped Unicode in a way that won't cause compile-time issues
            string jsonWithUnicodeIssues = "{\"Name\":\"TestInvalid\",\"Description\":\"Valid\"}";
            
            // Act
            var result = JsonUtilityHelper.FromJsonSafe<TestDataWithUnicode>(jsonWithUnicodeIssues);
            
            // Assert
            Assert.IsNotNull(result, "Should successfully deserialize");
            Assert.IsNotEmpty(result.Name, "Name should not be empty");
            Assert.AreEqual("Valid", result.Description, "Description should be preserved");
        }

        [Test]
        public void Test_JsonHelper_InvalidJsonHandling()
        {
            // Arrange
            string invalidJson = "{ invalid json structure";
            
            // Expect error log for invalid JSON - use regex pattern for flexibility
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Failed to deserialize JSON to TestDataWithUnicode.*JSON parse error.*Missing a name"));
            
            // Act
            var result = JsonUtilityHelper.FromJsonSafe<TestDataWithUnicode>(invalidJson);
            
            // Assert
            Assert.IsNull(result, "Should return null for invalid JSON");
        }

        #endregion

        #region Object Scanning Tests

        [Test]
        public void Test_ObjectScanning_DetectsIssues()
        {
            // Arrange
            var testObject = new TestDataWithUnicode
            {
                Name = "Test" + (char)0xD800 + "Invalid",
                Description = "Valid description",
                Tags = new List<string> { "Valid", "Test" + (char)0xDC00 + "Orphaned" }
            };
            
            // Act
            var report = UnicodeDataDiagnostic.ScanObjectForUnicodeIssues(testObject);
            
            // Assert
            Assert.IsTrue(report.HasIssues, "Should detect Unicode issues in object");
            Assert.GreaterOrEqual(report.IssueCount, 1, "Should detect at least 1 issue");
        }

        [Test]
        public void Test_ObjectScanning_NoIssues()
        {
            // Arrange
            var testObject = new TestDataWithUnicode
            {
                Name = "Valid Name",
                Description = "Valid description with emoji 🌱",
                Tags = new List<string> { "Valid", "Unicode🍃" }
            };
            
            // Act
            var report = UnicodeDataDiagnostic.ScanObjectForUnicodeIssues(testObject);
            
            // Assert
            Assert.IsFalse(report.HasIssues, "Should not detect issues in valid object");
            Assert.AreEqual(0, report.IssueCount, "Should have zero issues");
        }

        #endregion

        #region Integration Tests

        [Test]
        public void Test_GeneticData_UnicodeHandling()
        {
            // Arrange
            var genotype = new CannabisGenotype
            {
                StrainName = "Test" + (char)0xD800 + "Strain", // Invalid Unicode
                GenotypeId = "GENO_001",
                StrainId = "Valid ID"
            };
            
            // Act
            string json = JsonUtilityHelper.ToJsonSafe(genotype, true);
            var deserializedGenotype = JsonUtilityHelper.FromJsonSafe<CannabisGenotype>(json);
            
            // Assert
            Assert.IsNotNull(deserializedGenotype, "Should successfully serialize and deserialize");
            Assert.AreEqual("GENO_001", deserializedGenotype.GenotypeId, "ID should be preserved");
            Assert.IsNotEmpty(deserializedGenotype.StrainName, "Name should not be empty after sanitization");
            
            // Verify no Unicode issues in the final JSON
            Assert.IsFalse(UnicodeDataDiagnostic.HasUnicodeIssues(json, out var issues),
                "Final JSON should not have Unicode issues");
        }

        [Test]
        public void Test_LargeDataset_Performance()
        {
            // Arrange
            var largeDataset = new List<TestDataWithUnicode>();
            for (int i = 0; i < 1000; i++)
            {
                largeDataset.Add(new TestDataWithUnicode
                {
                    Name = $"Item {i}",
                    Description = $"Description for item {i} with emoji 🌱",
                    Tags = new List<string> { $"Tag{i}", "Unicode🍃" }
                });
            }
            
            // Add some items with Unicode issues
            largeDataset.Add(new TestDataWithUnicode
            {
                Name = "Test" + (char)0xD800 + "Invalid",
                Description = "Item with Unicode issues"
            });
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Act
            string json = JsonUtilityHelper.ToJsonSafe(largeDataset, false);
            stopwatch.Stop();
            
            // Assert
            Assert.IsNotEmpty(json, "Should produce JSON for large dataset");
            Assert.IsTrue(JsonUtilityHelper.IsValidJson(json), "Large dataset JSON should be valid");
            Assert.Less(stopwatch.ElapsedMilliseconds, 5000, "Should complete within 5 seconds");
            
            Debug.Log($"Large dataset serialization time: {stopwatch.ElapsedMilliseconds}ms, JSON size: {JsonUtilityHelper.GetJsonSizeInBytes(json)} bytes");
        }

        #endregion

        #region Utility Tests

        [Test]
        public void Test_JsonCompaction()
        {
            // Arrange
            string jsonWithWhitespace = @"
            {
                ""Name"": ""Test Name"",
                ""Description"": ""Test Description"",
                ""Tags"": [
                    ""Tag1"",
                    ""Tag2""
                ]
            }";
            
            // Act
            string compactJson = JsonUtilityHelper.CompactJson(jsonWithWhitespace);
            
            // Assert
            Assert.IsNotEmpty(compactJson, "Should produce compacted JSON");
            Assert.Less(compactJson.Length, jsonWithWhitespace.Length, "Compacted JSON should be smaller");
            Assert.IsTrue(JsonUtilityHelper.IsValidJson(compactJson), "Compacted JSON should still be valid");
        }

        #endregion
    }
} 