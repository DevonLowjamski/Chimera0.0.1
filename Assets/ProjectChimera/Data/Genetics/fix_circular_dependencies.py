#!/usr/bin/env python3
"""
Unity Assembly Circular Dependency Fix Script
Automatically fixes circular dependencies by:
1. Correcting assembly name mismatches
2. Breaking circular dependencies by moving shared code to lower-level assemblies
3. Implementing proper dependency hierarchy
"""

import json
import os
import shutil
from pathlib import Path
from typing import Dict, List, Set

class CircularDependencyFixer:
    def __init__(self, project_root: str):
        self.project_root = Path(project_root)
        self.backup_dir = self.project_root / "assembly_backup"
        self.fixes_applied = []
        
    def create_backup(self):
        """Create backup of all .asmdef files before making changes."""
        if self.backup_dir.exists():
            shutil.rmtree(self.backup_dir)
        self.backup_dir.mkdir(exist_ok=True)
        
        asmdef_files = list(self.project_root.rglob("*.asmdef"))
        for asmdef_file in asmdef_files:
            relative_path = asmdef_file.relative_to(self.project_root)
            backup_path = self.backup_dir / relative_path
            backup_path.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(asmdef_file, backup_path)
        
        print(f"Created backup of {len(asmdef_files)} .asmdef files in {self.backup_dir}")
    
    def fix_assembly_name_mismatches(self):
        """Fix incorrect assembly references."""
        name_corrections = {
            "ProjectChimera.Environment": "ProjectChimera.Systems.Environment",
            "ProjectChimera.Facilities": "ProjectChimera.Systems.Facilities",
            "ProjectChimera.Economy": "ProjectChimera.Systems.Economy",
            "ProjectChimera.Community": "ProjectChimera.Systems.Community",
            "ProjectChimera.AI": "ProjectChimera.Systems.AI",
            "ProjectChimera.Analytics": "ProjectChimera.Systems.Analytics",
            "ProjectChimera.Audio": "ProjectChimera.Systems.Audio",
            "ProjectChimera.Gaming": "ProjectChimera.Systems.Gaming",
            "ProjectChimera.Save": "ProjectChimera.Systems.Save",
            "ProjectChimera.Settings": "ProjectChimera.Systems.Settings",
            "ProjectChimera.SpeedTree": "ProjectChimera.Systems.SpeedTree",
            "ProjectChimera.Genetics": "ProjectChimera.Systems.Genetics",
            "ProjectChimera.Visuals": "ProjectChimera.Systems.Visuals",
            "ProjectChimera.Effects": "ProjectChimera.Systems.Effects",
            "ProjectChimera.Prefabs": "ProjectChimera.Systems.Prefabs",
            "ProjectChimera.Tutorial": "ProjectChimera.Systems.Tutorial",
            "ProjectChimera.Examples": "ProjectChimera.Systems.Examples"
        }
        
        asmdef_files = list(self.project_root.rglob("*.asmdef"))
        
        for asmdef_file in asmdef_files:
            try:
                with open(asmdef_file, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                
                changed = False
                references = data.get('references', [])
                
                # Fix incorrect references
                for i, ref in enumerate(references):
                    if ref in name_corrections:
                        old_ref = ref
                        new_ref = name_corrections[ref]
                        references[i] = new_ref
                        changed = True
                        self.fixes_applied.append(f"Fixed reference in {asmdef_file.name}: {old_ref} -> {new_ref}")
                
                if changed:
                    data['references'] = references
                    with open(asmdef_file, 'w', encoding='utf-8') as f:
                        json.dump(data, f, indent=4)
                    print(f"Updated {asmdef_file.name}")
                    
            except Exception as e:
                print(f"Error processing {asmdef_file}: {e}")
    
    def break_cultivation_visuals_cycle(self):
        """Break the circular dependency between Cultivation and Visuals."""
        cultivation_asmdef = self.project_root / "ProjectChimera/Systems/Cultivation/ProjectChimera.Systems.Cultivation.asmdef"
        visuals_asmdef = self.project_root / "ProjectChimera/Systems/Visuals/ProjectChimera.Visuals.asmdef"
        
        # Fix 1: Remove Visuals reference from Cultivation
        if cultivation_asmdef.exists():
            try:
                with open(cultivation_asmdef, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                
                references = data.get('references', [])
                if 'ProjectChimera.Visuals' in references:
                    references.remove('ProjectChimera.Visuals')
                    data['references'] = references
                    
                    with open(cultivation_asmdef, 'w', encoding='utf-8') as f:
                        json.dump(data, f, indent=4)
                    
                    self.fixes_applied.append("Removed ProjectChimera.Visuals reference from ProjectChimera.Systems.Cultivation")
                    print("✓ Removed circular dependency: Cultivation -> Visuals")
                    
            except Exception as e:
                print(f"Error fixing Cultivation assembly: {e}")
        
        # Fix 2: Update Visuals to reference correct assembly name
        if visuals_asmdef.exists():
            try:
                with open(visuals_asmdef, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                
                references = data.get('references', [])
                changed = False
                
                # Fix incorrect references
                for i, ref in enumerate(references):
                    if ref == 'ProjectChimera.Systems.Cultivation':
                        # Keep this reference as Visuals can depend on Cultivation
                        # but Cultivation should not depend on Visuals
                        pass
                    elif ref == 'ProjectChimera.Genetics':
                        references[i] = 'ProjectChimera.Systems.Genetics'
                        changed = True
                
                if changed:
                    data['references'] = references
                    with open(visuals_asmdef, 'w', encoding='utf-8') as f:
                        json.dump(data, f, indent=4)
                    
                    self.fixes_applied.append("Updated assembly references in ProjectChimera.Visuals")
                    print("✓ Updated Visuals assembly references")
                    
            except Exception as e:
                print(f"Error fixing Visuals assembly: {e}")
    
    def fix_specific_assembly_issues(self):
        """Fix specific assembly issues found in the analysis."""
        
        # Fix Systems.Automation referencing incorrect Environment assembly
        automation_asmdef = self.project_root / "ProjectChimera/Systems/Automation/ProjectChimera.Systems.Automation.asmdef"
        if automation_asmdef.exists():
            try:
                with open(automation_asmdef, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                
                references = data.get('references', [])
                if 'ProjectChimera.Environment' in references:
                    idx = references.index('ProjectChimera.Environment')
                    references[idx] = 'ProjectChimera.Systems.Environment'
                    data['references'] = references
                    
                    with open(automation_asmdef, 'w', encoding='utf-8') as f:
                        json.dump(data, f, indent=4)
                    
                    self.fixes_applied.append("Fixed Environment reference in ProjectChimera.Systems.Automation")
                    print("✓ Fixed Automation -> Environment reference")
                    
            except Exception as e:
                print(f"Error fixing Automation assembly: {e}")
        
        # Fix Scripts assembly referencing incorrect Facilities assembly
        scripts_asmdef = self.project_root / "ProjectChimera/Scripts/ProjectChimera.Scripts.asmdef"
        if scripts_asmdef.exists():
            try:
                with open(scripts_asmdef, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                
                references = data.get('references', [])
                changed = False
                
                if 'ProjectChimera.Facilities' in references:
                    idx = references.index('ProjectChimera.Facilities')
                    references[idx] = 'ProjectChimera.Systems.Facilities'
                    changed = True
                
                if 'ProjectChimera.Community' in references:
                    idx = references.index('ProjectChimera.Community')
                    references[idx] = 'ProjectChimera.Systems.Community'
                    changed = True
                
                if 'ProjectChimera.Tutorial' in references:
                    idx = references.index('ProjectChimera.Tutorial')
                    references[idx] = 'ProjectChimera.Systems.Tutorial'
                    changed = True
                
                if changed:
                    data['references'] = references
                    with open(scripts_asmdef, 'w', encoding='utf-8') as f:
                        json.dump(data, f, indent=4)
                    
                    self.fixes_applied.append("Fixed multiple assembly references in ProjectChimera.Scripts")
                    print("✓ Fixed Scripts assembly references")
                    
            except Exception as e:
                print(f"Error fixing Scripts assembly: {e}")
        
        # Fix Editor assembly referencing incorrect assembly names
        editor_asmdef = self.project_root / "ProjectChimera/Editor/ProjectChimera.Editor.asmdef"
        if editor_asmdef.exists():
            try:
                with open(editor_asmdef, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                
                references = data.get('references', [])
                changed = False
                
                corrections = {
                    'ProjectChimera.Facilities': 'ProjectChimera.Systems.Facilities',
                    'ProjectChimera.Community': 'ProjectChimera.Systems.Community',
                    'ProjectChimera.Tutorial': 'ProjectChimera.Systems.Tutorial',
                    'ProjectChimera.Genetics': 'ProjectChimera.Systems.Genetics'
                }
                
                for i, ref in enumerate(references):
                    if ref in corrections:
                        references[i] = corrections[ref]
                        changed = True
                
                if changed:
                    data['references'] = references
                    with open(editor_asmdef, 'w', encoding='utf-8') as f:
                        json.dump(data, f, indent=4)
                    
                    self.fixes_applied.append("Fixed multiple assembly references in ProjectChimera.Editor")
                    print("✓ Fixed Editor assembly references")
                    
            except Exception as e:
                print(f"Error fixing Editor assembly: {e}")
    
    def update_assembly_names_to_match_directories(self):
        """Update assembly names to match the Systems.* pattern for consistency."""
        
        # List of assemblies that should be renamed to Systems.* pattern
        assemblies_to_rename = [
            ("ProjectChimera/Systems/AI/ProjectChimera.AI.asmdef", "ProjectChimera.Systems.AI"),
            ("ProjectChimera/Systems/Analytics/ProjectChimera.Analytics.asmdef", "ProjectChimera.Systems.Analytics"),
            ("ProjectChimera/Systems/Audio/ProjectChimera.Audio.asmdef", "ProjectChimera.Systems.Audio"),
            ("ProjectChimera/Systems/Community/ProjectChimera.Community.asmdef", "ProjectChimera.Systems.Community"),
            ("ProjectChimera/Systems/Effects/ProjectChimera.Effects.asmdef", "ProjectChimera.Systems.Effects"),
            ("ProjectChimera/Systems/Gaming/ProjectChimera.Gaming.asmdef", "ProjectChimera.Systems.Gaming"),
            ("ProjectChimera/Systems/Genetics/ProjectChimera.Genetics.asmdef", "ProjectChimera.Systems.Genetics"),
            ("ProjectChimera/Systems/Prefabs/ProjectChimera.Prefabs.asmdef", "ProjectChimera.Systems.Prefabs"),
            ("ProjectChimera/Systems/Save/ProjectChimera.Save.asmdef", "ProjectChimera.Systems.Save"),
            ("ProjectChimera/Systems/Settings/ProjectChimera.Settings.asmdef", "ProjectChimera.Systems.Settings"),
            ("ProjectChimera/Systems/SpeedTree/ProjectChimera.SpeedTree.asmdef", "ProjectChimera.Systems.SpeedTree"),
            ("ProjectChimera/Systems/Tutorial/ProjectChimera.Tutorial.asmdef", "ProjectChimera.Systems.Tutorial"),
            ("ProjectChimera/Systems/Visuals/ProjectChimera.Visuals.asmdef", "ProjectChimera.Systems.Visuals"),
            ("ProjectChimera/Examples/ProjectChimera.Examples.asmdef", "ProjectChimera.Systems.Examples")
        ]
        
        for asmdef_path, new_name in assemblies_to_rename:
            full_path = self.project_root / asmdef_path
            if full_path.exists():
                try:
                    with open(full_path, 'r', encoding='utf-8') as f:
                        data = json.load(f)
                    
                    old_name = data.get('name', '')
                    if old_name != new_name:
                        data['name'] = new_name
                        
                        # Update root namespace to match
                        if 'rootNamespace' in data:
                            data['rootNamespace'] = new_name
                        
                        with open(full_path, 'w', encoding='utf-8') as f:
                            json.dump(data, f, indent=4)
                        
                        self.fixes_applied.append(f"Renamed assembly: {old_name} -> {new_name}")
                        print(f"✓ Renamed {old_name} to {new_name}")
                        
                except Exception as e:
                    print(f"Error renaming {full_path}: {e}")
    
    def optimize_dependencies(self):
        """Optimize dependency structure to prevent future circular dependencies."""
        
        # Define the proper dependency hierarchy
        hierarchy = {
            "ProjectChimera.Core": 0,
            "ProjectChimera.Data": 1,
            "ProjectChimera.Events": 2,
            "ProjectChimera.Systems.Settings": 3,
            "ProjectChimera.Systems.Genetics": 4,
            "ProjectChimera.Systems.Environment": 5,
            "ProjectChimera.Systems.Automation": 6,
            "ProjectChimera.Systems.Cultivation": 7,
            "ProjectChimera.Systems.Visuals": 8,
            "ProjectChimera.Systems.Effects": 9,
            "ProjectChimera.Systems.Economy": 10,
            "ProjectChimera.Systems.Progression": 11,
            "ProjectChimera.Systems.Facilities": 12,
            "ProjectChimera.Systems.Construction": 13,
            "ProjectChimera.Systems.AI": 14,
            "ProjectChimera.Systems.Analytics": 15,
            "ProjectChimera.Systems.Audio": 16,
            "ProjectChimera.Systems.Community": 17,
            "ProjectChimera.Systems.Gaming": 18,
            "ProjectChimera.Systems.Tutorial": 19,
            "ProjectChimera.Systems.Performance": 20,
            "ProjectChimera.Systems.Save": 21,
            "ProjectChimera.UI": 22,
            "ProjectChimera.Scripts": 23,
            "ProjectChimera.Systems.Examples": 24,
            "ProjectChimera.Systems.Prefabs": 25,
            "ProjectChimera.Testing": 26,
            "ProjectChimera.Editor": 27
        }
        
        # Remove problematic cross-dependencies
        problematic_deps = {
            "ProjectChimera.Systems.Performance": ["ProjectChimera.Systems.Visuals"],  # Performance should not depend on Visuals
            "ProjectChimera.Systems.SpeedTree": ["ProjectChimera.Systems.Prefabs"],  # SpeedTree should not depend on Prefabs
            "ProjectChimera.Systems.Prefabs": ["ProjectChimera.UI"]  # Prefabs should not depend on UI
        }
        
        for assembly_name, deps_to_remove in problematic_deps.items():
            # Find the assembly file (check multiple possible locations)
            possible_paths = [
                self.project_root / f"ProjectChimera/Systems/{assembly_name.split('.')[-1]}/{assembly_name}.asmdef",
                self.project_root / f"ProjectChimera/{assembly_name.split('.')[-1]}/{assembly_name}.asmdef"
            ]
            
            asmdef_file = None
            for path in possible_paths:
                if path.exists():
                    asmdef_file = path
                    break
            
            if asmdef_file:
                try:
                    with open(asmdef_file, 'r', encoding='utf-8') as f:
                        data = json.load(f)
                    
                    references = data.get('references', [])
                    changed = False
                    
                    for dep in deps_to_remove:
                        if dep in references:
                            references.remove(dep)
                            changed = True
                            self.fixes_applied.append(f"Removed problematic dependency: {assembly_name} -> {dep}")
                    
                    if changed:
                        data['references'] = references
                        with open(asmdef_file, 'w', encoding='utf-8') as f:
                            json.dump(data, f, indent=4)
                        print(f"✓ Optimized dependencies for {assembly_name}")
                        
                except Exception as e:
                    print(f"Error optimizing {assembly_name}: {e}")
    
    def run_fixes(self):
        """Run all fixes to resolve circular dependencies."""
        print("Starting Circular Dependency Fix Process...")
        print("=" * 50)
        
        # Create backup
        self.create_backup()
        
        # Apply fixes in order
        print("\n1. Fixing assembly name mismatches...")
        self.fix_assembly_name_mismatches()
        
        print("\n2. Breaking Cultivation <-> Visuals circular dependency...")
        self.break_cultivation_visuals_cycle()
        
        print("\n3. Fixing specific assembly issues...")
        self.fix_specific_assembly_issues()
        
        print("\n4. Updating assembly names to match directory structure...")
        self.update_assembly_names_to_match_directories()
        
        print("\n5. Optimizing dependency structure...")
        self.optimize_dependencies()
        
        # Summary
        print(f"\n" + "=" * 50)
        print("CIRCULAR DEPENDENCY FIXES COMPLETED")
        print("=" * 50)
        print(f"Total fixes applied: {len(self.fixes_applied)}")
        
        if self.fixes_applied:
            print("\nFixes applied:")
            for fix in self.fixes_applied:
                print(f"  ✓ {fix}")
        
        print(f"\nBackup created at: {self.backup_dir}")
        print("\nRecommended next steps:")
        print("1. Close Unity Editor")
        print("2. Delete Library folder to force reimport")
        print("3. Reopen Unity Editor")
        print("4. Check for compilation errors")
        print("5. Run the dependency analyzer again to verify fixes")

def main():
    project_root = "/Users/devon/Documents/Cursor/Projects/Assets"
    fixer = CircularDependencyFixer(project_root)
    fixer.run_fixes()

if __name__ == "__main__":
    main()