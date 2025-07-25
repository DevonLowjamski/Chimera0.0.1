#!/usr/bin/env python3
"""
Project Chimera: Comprehensive Code Audit Script
Phase 1.2: Complete Code Audit & Inventory

This script automatically discovers and catalogs all problematic files identified
in the architectural decisions for deletion or refactoring.
"""

import os
import re
import json
from pathlib import Path
from datetime import datetime
from typing import Dict, List, Tuple

class ProjectChimeraAudit:
    def __init__(self, project_root: str):
        self.project_root = Path(project_root)
        self.assets_path = self.project_root / "Assets" / "ProjectChimera"
        self.audit_results = {
            "timestamp": datetime.now().isoformat(),
            "disabled_files": [],
            "backup_files": [],
            "redundant_managers": [],
            "monolithic_classes": [],
            "namespace_conflicts": [],
            "deletion_targets": [],
            "summary": {}
        }
        
        # Based on ADR decisions
        self.deletion_targets = {
            "save_managers": ["Assets/ProjectChimera/Core/SaveManager.cs"],
            "progression_managers": [
                "Assets/ProjectChimera/Systems/Progression/ProgressionManager.cs",
                "Assets/ProjectChimera/Systems/Progression/CleanProgressionManager.cs", 
                "Assets/ProjectChimera/Systems/Progression/MilestoneProgressionSystem.cs"
            ],
            "ui_managers": [
                "Assets/ProjectChimera/UI/Core/GameUIManager.cs",
                "Assets/ProjectChimera/UI/AdvancedUIManager.cs"
            ],
            "achievement_managers": [
                "Assets/ProjectChimera/Systems/Progression/AchievementManager.cs"
            ]
        }

    def find_disabled_files(self) -> List[Dict]:
        """Find all .cs.disabled files"""
        disabled_files = []
        for file_path in self.assets_path.rglob("*.cs.disabled"):
            file_info = {
                "path": str(file_path.relative_to(self.project_root)),
                "absolute_path": str(file_path),
                "size_bytes": file_path.stat().st_size,
                "last_modified": datetime.fromtimestamp(file_path.stat().st_mtime).isoformat()
            }
            disabled_files.append(file_info)
        return disabled_files

    def find_backup_files(self) -> List[Dict]:
        """Find all .backup files"""
        backup_files = []
        patterns = ["*.backup", "*.backup2", "*.bak"]
        
        for pattern in patterns:
            for file_path in self.assets_path.rglob(pattern):
                file_info = {
                    "path": str(file_path.relative_to(self.project_root)),
                    "absolute_path": str(file_path),
                    "size_bytes": file_path.stat().st_size,
                    "last_modified": datetime.fromtimestamp(file_path.stat().st_mtime).isoformat()
                }
                backup_files.append(file_info)
        return backup_files

    def find_redundant_managers(self) -> List[Dict]:
        """Find duplicate manager implementations"""
        redundant_managers = []
        
        # Look for files ending with Manager.cs
        manager_files = list(self.assets_path.rglob("*Manager.cs"))
        
        # Group by base name to find duplicates
        name_groups = {}
        for file_path in manager_files:
            base_name = file_path.stem
            if base_name not in name_groups:
                name_groups[base_name] = []
            name_groups[base_name].append(file_path)
        
        # Find groups with more than one file
        for base_name, file_list in name_groups.items():
            if len(file_list) > 1:
                manager_group = {
                    "manager_name": base_name,
                    "duplicates": []
                }
                for file_path in file_list:
                    file_info = {
                        "path": str(file_path.relative_to(self.project_root)),
                        "absolute_path": str(file_path),
                        "size_bytes": file_path.stat().st_size,
                        "line_count": self.count_lines(file_path)
                    }
                    manager_group["duplicates"].append(file_info)
                redundant_managers.append(manager_group)
        
        return redundant_managers

    def find_monolithic_classes(self, line_threshold: int = 500) -> List[Dict]:
        """Find classes exceeding line count threshold"""
        monolithic_classes = []
        
        for file_path in self.assets_path.rglob("*.cs"):
            if file_path.suffix == ".cs":  # Exclude .cs.disabled
                line_count = self.count_lines(file_path)
                if line_count > line_threshold:
                    file_info = {
                        "path": str(file_path.relative_to(self.project_root)),
                        "absolute_path": str(file_path),
                        "line_count": line_count,
                        "size_bytes": file_path.stat().st_size,
                        "refactoring_priority": "high" if line_count > 1000 else "medium"
                    }
                    monolithic_classes.append(file_info)
        
        return sorted(monolithic_classes, key=lambda x: x["line_count"], reverse=True)

    def find_namespace_conflicts(self) -> List[Dict]:
        """Find potential namespace conflicts"""
        namespace_conflicts = []
        namespace_map = {}
        
        for file_path in self.assets_path.rglob("*.cs"):
            if file_path.suffix == ".cs":
                try:
                    with open(file_path, 'r', encoding='utf-8') as f:
                        content = f.read()
                    
                    # Find namespace declarations
                    namespace_matches = re.findall(r'namespace\s+([^\s{]+)', content)
                    
                    # Find using aliases
                    alias_matches = re.findall(r'using\s+(\w+)\s*=\s*([^;]+);', content)
                    
                    if namespace_matches or alias_matches:
                        file_info = {
                            "path": str(file_path.relative_to(self.project_root)),
                            "namespaces": namespace_matches,
                            "using_aliases": alias_matches
                        }
                        
                        # Check for conflicts
                        for ns in namespace_matches:
                            if ns in namespace_map:
                                conflict_info = {
                                    "namespace": ns,
                                    "files": [namespace_map[ns], file_info["path"]],
                                    "type": "duplicate_namespace"
                                }
                                namespace_conflicts.append(conflict_info)
                            else:
                                namespace_map[ns] = file_info["path"]
                        
                        if alias_matches:
                            namespace_conflicts.append({
                                "file": file_info["path"],
                                "aliases": alias_matches,
                                "type": "using_alias"
                            })
                
                except Exception as e:
                    print(f"Error reading {file_path}: {e}")
        
        return namespace_conflicts

    def compile_deletion_targets(self) -> List[Dict]:
        """Compile all files marked for deletion based on ADR decisions"""
        deletion_targets = []
        
        for category, file_paths in self.deletion_targets.items():
            for file_path in file_paths:
                full_path = self.project_root / file_path
                if full_path.exists():
                    file_info = {
                        "path": file_path,
                        "absolute_path": str(full_path),
                        "category": category,
                        "reason": self.get_deletion_reason(category),
                        "size_bytes": full_path.stat().st_size,
                        "line_count": self.count_lines(full_path)
                    }
                    deletion_targets.append(file_info)
                else:
                    # File doesn't exist, but mark as target
                    file_info = {
                        "path": file_path,
                        "absolute_path": str(full_path),
                        "category": category,
                        "reason": self.get_deletion_reason(category),
                        "status": "not_found"
                    }
                    deletion_targets.append(file_info)
        
        return deletion_targets

    def get_deletion_reason(self, category: str) -> str:
        """Get reason for deletion based on category"""
        reasons = {
            "save_managers": "ADR-001: Superseded by Systems/Save/SaveManager.cs",
            "progression_managers": "ADR-002: Superseded by ComprehensiveProgressionManager.cs",
            "ui_managers": "ADR-005: Superseded by UIManager.cs",
            "achievement_managers": "ADR-004: Superseded by LegacyAchievementManager.cs"
        }
        return reasons.get(category, "Marked for deletion in architectural decisions")

    def count_lines(self, file_path: Path) -> int:
        """Count lines in a file"""
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                return sum(1 for _ in f)
        except:
            return 0

    def run_audit(self) -> Dict:
        """Run complete audit and return results"""
        print("Starting Project Chimera Code Audit...")
        
        print("1. Finding disabled files...")
        self.audit_results["disabled_files"] = self.find_disabled_files()
        
        print("2. Finding backup files...")
        self.audit_results["backup_files"] = self.find_backup_files()
        
        print("3. Finding redundant managers...")
        self.audit_results["redundant_managers"] = self.find_redundant_managers()
        
        print("4. Finding monolithic classes...")
        self.audit_results["monolithic_classes"] = self.find_monolithic_classes()
        
        print("5. Finding namespace conflicts...")
        self.audit_results["namespace_conflicts"] = self.find_namespace_conflicts()
        
        print("6. Compiling deletion targets...")
        self.audit_results["deletion_targets"] = self.compile_deletion_targets()
        
        # Generate summary
        self.audit_results["summary"] = {
            "disabled_files_count": len(self.audit_results["disabled_files"]),
            "backup_files_count": len(self.audit_results["backup_files"]),
            "redundant_manager_groups": len(self.audit_results["redundant_managers"]),
            "monolithic_classes_count": len(self.audit_results["monolithic_classes"]),
            "namespace_conflicts_count": len(self.audit_results["namespace_conflicts"]),
            "deletion_targets_count": len(self.audit_results["deletion_targets"]),
            "total_problematic_files": (
                len(self.audit_results["disabled_files"]) +
                len(self.audit_results["backup_files"]) +
                len(self.audit_results["deletion_targets"])
            )
        }
        
        print(f"Audit complete! Found {self.audit_results['summary']['total_problematic_files']} problematic files.")
        return self.audit_results

    def save_results(self, output_file: str = "code_audit_results.json"):
        """Save audit results to file"""
        output_path = self.project_root / output_file
        with open(output_path, 'w') as f:
            json.dump(self.audit_results, f, indent=2)
        print(f"Audit results saved to: {output_path}")

    def generate_report(self, output_file: str = "CODE_AUDIT_REPORT.md"):
        """Generate human-readable markdown report"""
        output_path = self.project_root / output_file
        
        with open(output_path, 'w') as f:
            f.write("# Project Chimera: Code Audit Report\n\n")
            f.write(f"**Generated**: {self.audit_results['timestamp']}\n\n")
            
            # Summary
            f.write("## Summary\n\n")
            summary = self.audit_results['summary']
            f.write(f"- **Disabled Files**: {summary['disabled_files_count']}\n")
            f.write(f"- **Backup Files**: {summary['backup_files_count']}\n")
            f.write(f"- **Redundant Manager Groups**: {summary['redundant_manager_groups']}\n")
            f.write(f"- **Monolithic Classes**: {summary['monolithic_classes_count']}\n")
            f.write(f"- **Namespace Conflicts**: {summary['namespace_conflicts_count']}\n")
            f.write(f"- **Deletion Targets**: {summary['deletion_targets_count']}\n")
            f.write(f"- **Total Problematic Files**: {summary['total_problematic_files']}\n\n")
            
            # Deletion targets
            f.write("## Files Marked for Deletion (ADR Decisions)\n\n")
            for target in self.audit_results['deletion_targets']:
                f.write(f"- `{target['path']}` - {target['reason']}\n")
            f.write("\n")
            
            # Disabled files
            f.write("## Disabled Files (.cs.disabled)\n\n")
            for file_info in self.audit_results['disabled_files']:
                f.write(f"- `{file_info['path']}`\n")
            f.write("\n")
            
            # Backup files
            f.write("## Backup Files\n\n")
            for file_info in self.audit_results['backup_files']:
                f.write(f"- `{file_info['path']}`\n")
            f.write("\n")
            
            # Monolithic classes
            f.write("## Monolithic Classes (>500 lines)\n\n")
            for file_info in self.audit_results['monolithic_classes']:
                f.write(f"- `{file_info['path']}` - {file_info['line_count']} lines ({file_info['refactoring_priority']} priority)\n")
            f.write("\n")
        
        print(f"Audit report saved to: {output_path}")

if __name__ == "__main__":
    # Run the audit
    project_root = os.getcwd()
    auditor = ProjectChimeraAudit(project_root)
    
    results = auditor.run_audit()
    auditor.save_results()
    auditor.generate_report()
    
    print("\n=== AUDIT COMPLETE ===")
    print(f"Results saved to: code_audit_results.json")
    print(f"Report saved to: CODE_AUDIT_REPORT.md")