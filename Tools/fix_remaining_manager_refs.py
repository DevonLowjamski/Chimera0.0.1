#!/usr/bin/env python3
"""
Script to find and fix remaining manager references that weren't caught by previous scripts.
"""

import os
import re

def fix_remaining_manager_refs(file_path):
    """Fix remaining manager references in UI files."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        lines = content.split('\n')
        modified = False
        
        for i, line in enumerate(lines):
            original_line = line
            
            # Skip already commented lines
            if line.strip().startswith('//'):
                continue
            
            # Look for manager property/method access patterns
            manager_patterns = [
                r'_\w*Manager\.',  # _someManager.Property or _someManager.Method()
                r'_\w*Manager\[',  # _someManager[index]
                r'_\w*Manager\s*\?',  # _someManager?.Property
                r'typeof\(\w*Manager\)',  # typeof(SomeManager)
            ]
            
            for pattern in manager_patterns:
                if re.search(pattern, line):
                    # Comment out the line
                    indent = len(line) - len(line.lstrip())
                    lines[i] = ' ' * indent + '// ' + line.lstrip()
                    modified = True
                    print(f"  Commented manager reference at line {i+1}: {original_line.strip()}")
                    break
            
            # Look for manager type declarations in field/variable declarations
            manager_type_pattern = r'(\w*Manager)\s+\w+'
            match = re.search(manager_type_pattern, line)
            if match and not line.strip().startswith('//'):
                manager_type = match.group(1)
                if manager_type.endswith('Manager') and manager_type != 'GameManager':
                    # This might be a type that should be changed to a Controller
                    if 'AIAdvisorManager' in line:
                        lines[i] = line.replace('AIAdvisorManager', 'AIAdvisorController')
                        modified = True
                        print(f"  Changed AIAdvisorManager to AIAdvisorController at line {i+1}")
                    else:
                        # Comment out other manager type declarations
                        indent = len(line) - len(line.lstrip())
                        lines[i] = ' ' * indent + '// ' + line.lstrip()
                        modified = True
                        print(f"  Commented manager type declaration at line {i+1}: {original_line.strip()}")
        
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
                
                if fix_remaining_manager_refs(file_path):
                    files_modified += 1
    
    print(f"\nCompleted! Modified {files_modified} files.")
    print("All remaining manager references have been fixed.")

if __name__ == "__main__":
    main()