#!/usr/bin/env python3
"""
Script to comment out 'using ProjectChimera.Systems.*' statements in UI files
to break circular dependencies.
"""

import os
import re

def comment_systems_imports(file_path):
    """Comment out using ProjectChimera.Systems.* statements in a file."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Pattern to match using ProjectChimera.Systems.* statements
        pattern = r'^(\s*)(using ProjectChimera\.Systems\.[^;]+;)(.*)$'
        
        lines = content.split('\n')
        modified = False
        
        for i, line in enumerate(lines):
            match = re.match(pattern, line)
            if match:
                indent, using_statement, rest = match.groups()
                lines[i] = f"{indent}// {using_statement}{rest}"
                modified = True
                print(f"  Commented: {using_statement}")
        
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
                
                if comment_systems_imports(file_path):
                    files_modified += 1
    
    print(f"\nCompleted! Modified {files_modified} files.")
    print("All 'using ProjectChimera.Systems.*' statements have been commented out.")

if __name__ == "__main__":
    main()