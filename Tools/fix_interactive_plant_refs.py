#!/usr/bin/env python3
"""
Script to comment out InteractivePlantComponent references in AdvancedGrowLightSystem.cs
"""

import re

def fix_interactive_plant_refs():
    file_path = "Assets/ProjectChimera/Systems/Environment/AdvancedGrowLightSystem.cs"
    
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        lines = content.split('\n')
        modified = False
        
        for i, line in enumerate(lines):
            # Skip already commented lines
            if line.strip().startswith('//'):
                continue
                
            # Comment out lines containing InteractivePlantComponent
            if 'InteractivePlantComponent' in line:
                indent = len(line) - len(line.lstrip())
                lines[i] = ' ' * indent + '// ' + line.lstrip()
                modified = True
                print(f"  Commented line {i+1}: {line.strip()}")
        
        if modified:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.write('\n'.join(lines))
            print(f"Successfully modified {file_path}")
            return True
        else:
            print("No modifications needed")
            return False
        
    except Exception as e:
        print(f"Error processing {file_path}: {e}")
        return False

if __name__ == "__main__":
    fix_interactive_plant_refs()