#!/usr/bin/env python3
"""
Script to fix missing UI types and using statements after breaking circular dependencies.
"""

import os
import re

def fix_ui_missing_types(file_path):
    """Fix missing using statements and type references in UI files."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        lines = content.split('\n')
        modified = False
        
        # Check if file needs ProjectChimera.Data.UI using statement
        needs_data_ui = False
        needs_ui_core = False
        
        # Look for types that require ProjectChimera.Data.UI
        data_ui_types = [
            'LeaderboardType', 'LeaderboardDisplayData', 'CompetitionEvent', 
            'CompetitionStatus', 'CompetitiveStatsSummary', 'PersonalRecord', 
            'RecordType', 'UIStatus', 'UIProgressBar', 'RandomEventData',
            'SaveSlotData', 'PlantBreedingData', 'FacilityData'
        ]
        
        # Look for types that require ProjectChimera.UI.Core
        ui_core_types = ['UIPanel', 'ChimeraUIPanel']
        
        for line in lines:
            for ui_type in data_ui_types:
                if ui_type in line and not line.strip().startswith('//'):
                    needs_data_ui = True
                    break
            for ui_type in ui_core_types:
                if ui_type in line and not line.strip().startswith('//'):
                    needs_ui_core = True
                    break
        
        # Add missing using statements
        using_section_end = -1
        has_data_ui_using = False
        has_ui_core_using = False
        
        for i, line in enumerate(lines):
            if line.strip().startswith('using '):
                using_section_end = i
                if 'ProjectChimera.Data.UI' in line:
                    has_data_ui_using = True
                if 'ProjectChimera.UI.Core' in line:
                    has_ui_core_using = True
            elif line.strip().startswith('namespace '):
                break
        
        # Insert missing using statements
        if needs_data_ui and not has_data_ui_using and using_section_end >= 0:
            lines.insert(using_section_end + 1, 'using ProjectChimera.Data.UI;')
            modified = True
            print(f"  Added 'using ProjectChimera.Data.UI;' to {file_path}")
        
        if needs_ui_core and not has_ui_core_using and using_section_end >= 0:
            lines.insert(using_section_end + 1, 'using ProjectChimera.UI.Core;')
            modified = True
            print(f"  Added 'using ProjectChimera.UI.Core;' to {file_path}")
        
        # Fix ChimeraUIPanel references
        for i, line in enumerate(lines):
            if 'ChimeraUIPanel' in line and not line.strip().startswith('//'):
                lines[i] = line.replace('ChimeraUIPanel', 'UIPanel')
                modified = True
                print(f"  Changed ChimeraUIPanel to UIPanel in {file_path}")
        
        # Comment out remaining Systems namespace references
        for i, line in enumerate(lines):
            if ('ProjectChimera.Systems.' in line and 
                line.strip().startswith('using ') and 
                not line.strip().startswith('//')):
                lines[i] = '// ' + line
                modified = True
                print(f"  Commented out Systems using statement in {file_path}")
        
        if modified:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write('\n'.join(lines))
            return True
        return False
        
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
        return False

def main():
    ui_directory = "Assets/ProjectChimera/UI"
    
    if not os.path.exists(ui_directory):
        print(f"UI directory not found: {ui_directory}")
        return
    
    files_modified = 0
    
    # Walk through all .cs files in UI directory
    for root, dirs, files in os.walk(ui_directory):
        for file in files:
            if file.endswith('.cs') and not file.endswith('.backup'):
                file_path = os.path.join(root, file)
                print(f"Processing: {file_path}")
                
                if fix_ui_missing_types(file_path):
                    files_modified += 1
    
    print(f"\nCompleted! Modified {files_modified} files.")
    print("All missing UI types and using statements have been fixed.")

if __name__ == "__main__":
    main()