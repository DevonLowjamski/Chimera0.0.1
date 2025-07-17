using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using ProjectChimera.Core;
using ProjectChimera.Data.Tutorial;

namespace ProjectChimera.Systems.Tutorial.Testing
{
    /// <summary>
    /// Utilities for validating tutorial data and configurations.
    /// Provides comprehensive validation tools for tutorial content.
    /// </summary>
    public static class TutorialValidationUtilities
    {
        /// <summary>
        /// Validate tutorial sequence data
        /// </summary>
        public static TutorialValidationResult ValidateSequence(TutorialSequenceSO sequence)
        {
            var result = new TutorialValidationResult();
            
            if (sequence == null)
            {
                result.AddError("Sequence is null");
                return result;
            }
            
            // Validate basic properties
            if (string.IsNullOrEmpty(sequence.SequenceId))
            {
                result.AddError("Sequence ID is empty");
            }
            
            if (string.IsNullOrEmpty(sequence.SequenceName))
            {
                result.AddError("Sequence name is empty");
            }
            
            // Validate steps
            if (sequence.Steps == null || sequence.Steps.Count == 0)
            {
                result.AddError("Sequence has no steps");
            }
            else
            {
                // Check for null steps
                var nullSteps = sequence.Steps.Where(s => s == null).Count();
                if (nullSteps > 0)
                {
                    result.AddError($"Sequence contains {nullSteps} null steps");
                }
                
                // Validate step count
                if (sequence.StepCount != sequence.Steps.Count)
                {
                    result.AddWarning($"Step count mismatch: declared {sequence.StepCount}, actual {sequence.Steps.Count}");
                }
                
                // Validate individual steps
                var stepIds = new HashSet<string>();
                for (int i = 0; i < sequence.Steps.Count; i++)
                {
                    var step = sequence.Steps[i];
                    if (step != null)
                    {
                        var stepResult = ValidateStep(step);
                        result.MergeResult(stepResult, $"Step {i + 1}");
                        
                        // Check for duplicate step IDs
                        if (stepIds.Contains(step.StepId))
                        {
                            result.AddError($"Duplicate step ID: {step.StepId}");
                        }
                        else
                        {
                            stepIds.Add(step.StepId);
                        }
                    }
                }
            }
            
            // Validate estimated duration
            if (sequence.EstimatedDuration <= 0)
            {
                result.AddWarning("Sequence has no estimated duration");
            }
            
            return result;
        }
        
        /// <summary>
        /// Validate tutorial step data
        /// </summary>
        public static TutorialValidationResult ValidateStep(TutorialStepSO step)
        {
            var result = new TutorialValidationResult();
            
            if (step == null)
            {
                result.AddError("Step is null");
                return result;
            }
            
            // Validate basic properties
            if (string.IsNullOrEmpty(step.StepId))
            {
                result.AddError("Step ID is empty");
            }
            else if (!IsValidStepId(step.StepId))
            {
                result.AddWarning($"Step ID doesn't follow naming convention: {step.StepId}");
            }
            
            if (string.IsNullOrEmpty(step.Title))
            {
                result.AddError("Step title is empty");
            }
            
            if (string.IsNullOrEmpty(step.InstructionText))
            {
                result.AddWarning("Step has no instruction text");
            }
            
            // Validate step type and validation requirements
            ValidateStepTypeRequirements(step, result);
            
            // Validate validation settings
            ValidateStepValidation(step, result);
            
            // Validate timeout settings
            if (step.TimeoutDuration < 0)
            {
                result.AddError("Step timeout duration cannot be negative");
            }
            
            return result;
        }
        
        /// <summary>
        /// Validate step type requirements
        /// </summary>
        private static void ValidateStepTypeRequirements(TutorialStepSO step, TutorialValidationResult result)
        {
            switch (step.StepType)
            {
                case TutorialStepType.Assessment:
                    if (step.ValidationType == TutorialValidationType.None)
                    {
                        result.AddError("Assessment steps must have validation");
                    }
                    if (step.CanSkip)
                    {
                        result.AddWarning("Assessment steps should typically not be skippable");
                    }
                    break;
                    
                case TutorialStepType.Problem_Solving:
                    if (step.ValidationType == TutorialValidationType.None || 
                        step.ValidationType == TutorialValidationType.Timer)
                    {
                        result.AddWarning("Problem solving steps should have interactive validation");
                    }
                    break;
                    
                case TutorialStepType.Project:
                    if (step.TimeoutDuration == 0)
                    {
                        result.AddWarning("Project steps should have timeout duration");
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Validate step validation settings
        /// </summary>
        private static void ValidateStepValidation(TutorialStepSO step, TutorialValidationResult result)
        {
            switch (step.ValidationType)
            {
                case TutorialValidationType.ButtonClick:
                case TutorialValidationType.UIElementClick:
                case TutorialValidationType.GameObjectClick:
                    if (string.IsNullOrEmpty(step.ValidationTarget))
                    {
                        result.AddError($"Validation type {step.ValidationType} requires validation target");
                    }
                    break;
                    
                case TutorialValidationType.SystemEvent:
                    if (string.IsNullOrEmpty(step.ValidationTarget))
                    {
                        result.AddError("System event validation requires event name");
                    }
                    break;
                    
                case TutorialValidationType.Timer:
                    if (step.TimeoutDuration <= 0)
                    {
                        result.AddError("Timer validation requires timeout duration");
                    }
                    break;
                    
                case TutorialValidationType.PlayerInput:
                    if (string.IsNullOrEmpty(step.ValidationTarget))
                    {
                        result.AddWarning("Player input validation should specify expected input");
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Validate tutorial data asset manager
        /// </summary>
        public static TutorialValidationResult ValidateDataAssetManager(TutorialDataAssetManager manager)
        {
            var result = new TutorialValidationResult();
            
            if (manager == null)
            {
                result.AddError("Tutorial data asset manager is null");
                return result;
            }
            
            // Validate available sequences
            if (manager.AvailableSequences == null || manager.AvailableSequences.Count == 0)
            {
                result.AddError("No tutorial sequences available");
            }
            else
            {
                foreach (var sequence in manager.AvailableSequences)
                {
                    var sequenceResult = ValidateSequence(sequence);
                    result.MergeResult(sequenceResult, $"Sequence: {sequence?.SequenceName ?? "Unknown"}");
                }
            }
            
            // Validate specific sequences
            ValidateSpecificSequence(manager.OnboardingSequence, "Onboarding", result);
            ValidateSpecificSequence(manager.CultivationSequence, "Cultivation", result);
            ValidateSpecificSequence(manager.GeneticsSequence, "Genetics", result);
            ValidateSpecificSequence(manager.EconomicsSequence, "Economics", result);
            
            // Check for duplicate sequence IDs
            var sequenceIds = manager.AvailableSequences
                .Where(s => s != null)
                .Select(s => s.SequenceId)
                .ToList();
            
            var duplicateIds = sequenceIds.GroupBy(id => id)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);
            
            foreach (var duplicateId in duplicateIds)
            {
                result.AddError($"Duplicate sequence ID: {duplicateId}");
            }
            
            return result;
        }
        
        /// <summary>
        /// Validate specific sequence reference
        /// </summary>
        private static void ValidateSpecificSequence(TutorialSequenceSO sequence, string name, TutorialValidationResult result)
        {
            if (sequence == null)
            {
                result.AddWarning($"{name} sequence is not assigned");
            }
            else
            {
                var sequenceResult = ValidateSequence(sequence);
                result.MergeResult(sequenceResult, $"{name} Sequence");
            }
        }
        
        /// <summary>
        /// Check if step ID follows naming convention
        /// </summary>
        private static bool IsValidStepId(string stepId)
        {
            if (string.IsNullOrEmpty(stepId))
                return false;
            
            // Check basic format: lowercase with underscores
            if (stepId != stepId.ToLower())
                return false;
            
            // Should contain at least one underscore
            if (!stepId.Contains("_"))
                return false;
            
            // Should not start or end with underscore
            if (stepId.StartsWith("_") || stepId.EndsWith("_"))
                return false;
            
            // Should not contain double underscores
            if (stepId.Contains("__"))
                return false;
            
            return true;
        }
        
        /// <summary>
        /// Validate tutorial step data structure
        /// </summary>
        public static TutorialValidationResult ValidateStepData(TutorialStepData stepData)
        {
            var result = new TutorialValidationResult();
            
            if (string.IsNullOrEmpty(stepData.StepId))
            {
                result.AddError("Step data ID is empty");
            }
            
            if (string.IsNullOrEmpty(stepData.Title))
            {
                result.AddError("Step data title is empty");
            }
            
            if (string.IsNullOrEmpty(stepData.DetailedInstructions))
            {
                result.AddWarning("Step data has no detailed instructions");
            }
            
            // Validate validation settings
            if (stepData.ValidationType != TutorialValidationType.None && 
                stepData.ValidationType != TutorialValidationType.Timer &&
                string.IsNullOrEmpty(stepData.ValidationTarget))
            {
                result.AddError($"Step data validation type {stepData.ValidationType} requires target");
            }
            
            if (stepData.TimeoutDuration < 0)
            {
                result.AddError("Step data timeout duration cannot be negative");
            }
            
            return result;
        }
        
        /// <summary>
        /// Generate validation report for tutorial system
        /// </summary>
        public static string GenerateValidationReport(TutorialDataAssetManager manager)
        {
            var result = ValidateDataAssetManager(manager);
            return GenerateValidationReport(result);
        }
        
        /// <summary>
        /// Generate validation report from result
        /// </summary>
        public static string GenerateValidationReport(TutorialValidationResult result)
        {
            var report = new System.Text.StringBuilder();
            
            report.AppendLine("=== TUTORIAL VALIDATION REPORT ===");
            report.AppendLine($"Generated: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            
            report.AppendLine($"Total Issues: {result.ErrorCount + result.WarningCount}");
            report.AppendLine($"Errors: {result.ErrorCount}");
            report.AppendLine($"Warnings: {result.WarningCount}");
            report.AppendLine();
            
            if (result.ErrorCount > 0)
            {
                report.AppendLine("ERRORS:");
                foreach (var error in result.Errors)
                {
                    report.AppendLine($"  ❌ {error}");
                }
                report.AppendLine();
            }
            
            if (result.WarningCount > 0)
            {
                report.AppendLine("WARNINGS:");
                foreach (var warning in result.Warnings)
                {
                    report.AppendLine($"  ⚠️  {warning}");
                }
                report.AppendLine();
            }
            
            if (result.ErrorCount == 0 && result.WarningCount == 0)
            {
                report.AppendLine("✅ VALIDATION PASSED - No issues found!");
            }
            else if (result.ErrorCount == 0)
            {
                report.AppendLine("✅ VALIDATION PASSED with warnings");
            }
            else
            {
                report.AppendLine("❌ VALIDATION FAILED - Critical errors found");
            }
            
            report.AppendLine("=== END VALIDATION REPORT ===");
            
            return report.ToString();
        }
    }
    
    /// <summary>
    /// Tutorial validation result container
    /// </summary>
    public class TutorialValidationResult
    {
        public List<string> Errors { get; private set; }
        public List<string> Warnings { get; private set; }
        
        public int ErrorCount => Errors.Count;
        public int WarningCount => Warnings.Count;
        public bool IsValid => ErrorCount == 0;
        
        public TutorialValidationResult()
        {
            Errors = new List<string>();
            Warnings = new List<string>();
        }
        
        public void AddError(string error)
        {
            Errors.Add(error);
        }
        
        public void AddWarning(string warning)
        {
            Warnings.Add(warning);
        }
        
        public void MergeResult(TutorialValidationResult other, string prefix = "")
        {
            foreach (var error in other.Errors)
            {
                Errors.Add(string.IsNullOrEmpty(prefix) ? error : $"{prefix}: {error}");
            }
            
            foreach (var warning in other.Warnings)
            {
                Warnings.Add(string.IsNullOrEmpty(prefix) ? warning : $"{prefix}: {warning}");
            }
        }
    }
}