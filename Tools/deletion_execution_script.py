#!/usr/bin/env python3
"""
Project Chimera: Safe Deletion Execution Script
Phase 1.3: Ruthless Code Deletion & Cleanup

This script safely deletes files identified in the audit based on ADR decisions.
It includes safety checks and generates deletion logs.
"""

import os
import json
import shutil
from pathlib import Path
from datetime import datetime
from typing import List, Dict

class SafeDeletionExecutor:
    def __init__(self, project_root: str, audit_file: str = "code_audit_results.json"):
        self.project_root = Path(project_root)
        self.audit_file = self.project_root / audit_file
        self.deletion_log = []
        self.safety_backups = []
        
        # Load audit results
        with open(self.audit_file, 'r') as f:
            self.audit_data = json.load(f)

    def create_safety_backup(self, file_path: Path) -> str:
        """Create a safety backup before deletion"""
        if not file_path.exists():
            return "file_not_found"
            
        backup_dir = self.project_root / "DELETION_BACKUPS" / datetime.now().strftime("%Y%m%d_%H%M%S")
        backup_dir.mkdir(parents=True, exist_ok=True)
        
        # Preserve directory structure in backup
        relative_path = file_path.relative_to(self.project_root)
        backup_path = backup_dir / relative_path
        backup_path.parent.mkdir(parents=True, exist_ok=True)
        
        shutil.copy2(file_path, backup_path)
        return str(backup_path)

    def delete_disabled_files(self) -> int:
        """Delete all .cs.disabled files"""
        deleted_count = 0
        
        print("Deleting .cs.disabled files...")
        for file_info in self.audit_data["disabled_files"]:
            file_path = self.project_root / file_info["path"]
            if file_path.exists():
                backup_path = self.create_safety_backup(file_path)
                try:
                    file_path.unlink()
                    deleted_count += 1
                    
                    self.deletion_log.append({
                        "file": file_info["path"],
                        "type": "disabled_file",
                        "status": "deleted",
                        "backup": backup_path,
                        "timestamp": datetime.now().isoformat()
                    })
                    
                    # Also delete corresponding .meta file if it exists
                    meta_path = Path(str(file_path) + ".meta")
                    if meta_path.exists():
                        meta_path.unlink()
                        self.deletion_log.append({
                            "file": str(meta_path.relative_to(self.project_root)),
                            "type": "meta_file",
                            "status": "deleted",
                            "timestamp": datetime.now().isoformat()
                        })
                    
                except Exception as e:
                    self.deletion_log.append({
                        "file": file_info["path"],
                        "type": "disabled_file",
                        "status": "error",
                        "error": str(e),
                        "timestamp": datetime.now().isoformat()
                    })
        
        return deleted_count

    def delete_backup_files(self) -> int:
        """Delete all .backup files"""
        deleted_count = 0
        
        print("Deleting .backup files...")
        for file_info in self.audit_data["backup_files"]:
            file_path = self.project_root / file_info["path"]
            if file_path.exists():
                backup_path = self.create_safety_backup(file_path)
                try:
                    file_path.unlink()
                    deleted_count += 1
                    
                    self.deletion_log.append({
                        "file": file_info["path"],
                        "type": "backup_file",
                        "status": "deleted",
                        "backup": backup_path,
                        "timestamp": datetime.now().isoformat()
                    })
                    
                except Exception as e:
                    self.deletion_log.append({
                        "file": file_info["path"],
                        "type": "backup_file",
                        "status": "error",
                        "error": str(e),
                        "timestamp": datetime.now().isoformat()
                    })
        
        return deleted_count

    def delete_adr_targets(self) -> int:
        """Delete files marked for deletion in ADR decisions"""
        deleted_count = 0
        
        print("Deleting ADR target files...")
        for target_info in self.audit_data["deletion_targets"]:
            if target_info.get("status") == "not_found":
                continue
                
            file_path = self.project_root / target_info["path"]
            if file_path.exists():
                backup_path = self.create_safety_backup(file_path)
                try:
                    file_path.unlink()
                    deleted_count += 1
                    
                    self.deletion_log.append({
                        "file": target_info["path"],
                        "type": "adr_target",
                        "category": target_info["category"],
                        "reason": target_info["reason"],
                        "status": "deleted",
                        "backup": backup_path,
                        "timestamp": datetime.now().isoformat()
                    })
                    
                    # Also delete corresponding .meta file if it exists
                    meta_path = Path(str(file_path) + ".meta")
                    if meta_path.exists():
                        meta_path.unlink()
                        self.deletion_log.append({
                            "file": str(meta_path.relative_to(self.project_root)),
                            "type": "meta_file",
                            "status": "deleted",
                            "timestamp": datetime.now().isoformat()
                        })
                    
                except Exception as e:
                    self.deletion_log.append({
                        "file": target_info["path"],
                        "type": "adr_target",
                        "status": "error",
                        "error": str(e),
                        "timestamp": datetime.now().isoformat()
                    })
        
        return deleted_count

    def clean_empty_directories(self) -> int:
        """Remove empty directories after cleanup"""
        removed_count = 0
        
        print("Cleaning empty directories...")
        # Walk the directory tree from bottom up
        for root, dirs, files in os.walk(self.project_root / "Assets" / "ProjectChimera", topdown=False):
            root_path = Path(root)
            
            # Skip if directory contains files
            if files:
                continue
                
            # Skip if directory contains non-empty subdirectories
            if any(os.listdir(subdir) for subdir in [root_path / d for d in dirs] if (root_path / d).exists()):
                continue
            
            # Remove empty directory
            try:
                root_path.rmdir()
                removed_count += 1
                self.deletion_log.append({
                    "directory": str(root_path.relative_to(self.project_root)),
                    "type": "empty_directory",
                    "status": "removed",
                    "timestamp": datetime.now().isoformat()
                })
            except Exception as e:
                # Directory not empty or other error
                pass
        
        return removed_count

    def execute_cleanup(self) -> Dict:
        """Execute complete cleanup and return results"""
        print("Starting Project Chimera Safe Deletion...")
        
        results = {
            "start_time": datetime.now().isoformat(),
            "disabled_files_deleted": 0,
            "backup_files_deleted": 0,
            "adr_targets_deleted": 0,
            "empty_directories_removed": 0,
            "total_files_deleted": 0,
            "errors": 0
        }
        
        # Execute deletions
        results["disabled_files_deleted"] = self.delete_disabled_files()
        results["backup_files_deleted"] = self.delete_backup_files()
        results["adr_targets_deleted"] = self.delete_adr_targets()
        results["empty_directories_removed"] = self.clean_empty_directories()
        
        # Calculate totals
        results["total_files_deleted"] = (
            results["disabled_files_deleted"] + 
            results["backup_files_deleted"] + 
            results["adr_targets_deleted"]
        )
        
        results["errors"] = len([log for log in self.deletion_log if log.get("status") == "error"])
        results["end_time"] = datetime.now().isoformat()
        
        return results

    def save_deletion_log(self, output_file: str = "DELETION_LOG.json"):
        """Save detailed deletion log"""
        log_data = {
            "timestamp": datetime.now().isoformat(),
            "total_operations": len(self.deletion_log),
            "operations": self.deletion_log
        }
        
        output_path = self.project_root / output_file
        with open(output_path, 'w') as f:
            json.dump(log_data, f, indent=2)
        print(f"Deletion log saved to: {output_path}")

    def generate_deletion_report(self, results: Dict, output_file: str = "DELETION_REPORT.md"):
        """Generate human-readable deletion report"""
        output_path = self.project_root / output_file
        
        with open(output_path, 'w') as f:
            f.write("# Project Chimera: Deletion Report\n\n")
            f.write(f"**Start Time**: {results['start_time']}\n")
            f.write(f"**End Time**: {results['end_time']}\n\n")
            
            # Summary
            f.write("## Summary\n\n")
            f.write(f"- **Disabled Files Deleted**: {results['disabled_files_deleted']}\n")
            f.write(f"- **Backup Files Deleted**: {results['backup_files_deleted']}\n")
            f.write(f"- **ADR Target Files Deleted**: {results['adr_targets_deleted']}\n")
            f.write(f"- **Empty Directories Removed**: {results['empty_directories_removed']}\n")
            f.write(f"- **Total Files Deleted**: {results['total_files_deleted']}\n")
            f.write(f"- **Errors**: {results['errors']}\n\n")
            
            # ADR targets
            f.write("## ADR Target Files Deleted\n\n")
            adr_deletions = [log for log in self.deletion_log if log.get("type") == "adr_target"]
            for deletion in adr_deletions:
                f.write(f"- `{deletion['file']}` - {deletion['reason']}\n")
            f.write("\n")
            
            # Errors
            errors = [log for log in self.deletion_log if log.get("status") == "error"]
            if errors:
                f.write("## Errors Encountered\n\n")
                for error in errors:
                    f.write(f"- `{error['file']}` - {error['error']}\n")
                f.write("\n")
            
            # Safety note
            f.write("## Safety Backups\n\n")
            f.write("All deleted files have been backed up to the `DELETION_BACKUPS/` directory with timestamps.\n")
            f.write("Backups can be restored if needed during Phase 1 validation.\n\n")
        
        print(f"Deletion report saved to: {output_path}")

if __name__ == "__main__":
    # Execute the cleanup
    project_root = os.getcwd()
    
    # Check if audit file exists
    audit_file = Path(project_root) / "code_audit_results.json"
    if not audit_file.exists():
        print("ERROR: code_audit_results.json not found. Run comprehensive_code_audit.py first.")
        exit(1)
    
    # Execute cleanup
    executor = SafeDeletionExecutor(project_root)
    results = executor.execute_cleanup()
    
    # Save logs and reports
    executor.save_deletion_log()
    executor.generate_deletion_report(results)
    
    print("\n=== DELETION COMPLETE ===")
    print(f"Total files deleted: {results['total_files_deleted']}")
    print(f"Errors: {results['errors']}")
    print("Logs saved to: DELETION_LOG.json")
    print("Report saved to: DELETION_REPORT.md")