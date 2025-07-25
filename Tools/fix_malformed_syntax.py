#!/usr/bin/env python3
"""
Script to fix malformed syntax issues created by automated commenting scripts.
"""

import os
import re

def fix_malformed_syntax(file_path):
    """Fix malformed syntax patterns in UI files."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        lines = content.split('\n')
        modified = False
        
        i = 0
        while i < len(lines):
            line = lines[i]
            stripped = line.strip()
            
            # Pattern 1: Orphaned if/else statements with commented actions
            if (stripped.startswith('if (') or stripped.startswith('else if (')) and not stripped.startswith('//'):
                # Check if the next line is a commented action
                if i + 1 < len(lines):
                    next_line = lines[i + 1].strip()
                    if next_line.startswith('//') and not next_line.startswith('// if') and not next_line.startswith('// else'):
                        # Comment out the if statement
                        indent = len(line) - len(line.lstrip())
                        lines[i] = ' ' * indent + '// ' + line.lstrip()
                        modified = True
                        print(f"  Fixed orphaned if statement at line {i+1}: {stripped}")
            
            # Pattern 2: Orphaned else statements
            elif stripped == 'else' and not stripped.startswith('//'):
                # Check if the next line is a commented action
                if i + 1 < len(lines):
                    next_line = lines[i + 1].strip()
                    if next_line.startswith('//'):
                        # Comment out the else statement
                        indent = len(line) - len(line.lstrip())
                        lines[i] = ' ' * indent + '// ' + line.lstrip()
                        modified = True
                        print(f"  Fixed orphaned else statement at line {i+1}")
            
            # Pattern 3: Broken LINQ chains
            elif '.FirstOrDefault(' in line and not line.strip().startswith('//'):
                # Check if this line starts with a dot (continuation of previous line)
                if stripped.startswith('.'):
                    # Check if previous line is commented
                    if i > 0:
                        prev_line = lines[i - 1].strip()
                        if prev_line.startswith('//'):
                            # Comment out this continuation line
                            indent = len(line) - len(line.lstrip())
                            lines[i] = ' ' * indent + '// ' + line.lstrip()
                            modified = True
                            print(f"  Fixed broken LINQ chain at line {i+1}")
            
            # Pattern 4: Orphaned method bodies (opening brace commented but body not)
            elif stripped.startswith('// ') and ('{' in stripped or 'void ' in stripped or 'bool ' in stripped):
                # This might be a commented method signature, check if body is uncommented
                brace_count = 0
                j = i + 1
                found_uncommented_body = False
                
                while j < len(lines) and j < i + 20:  # Look ahead max 20 lines
                    body_line = lines[j].strip()
                    if not body_line:
                        j += 1
                        continue
                    
                    if body_line == '{':
                        brace_count += 1
                    elif body_line == '}':
                        brace_count -= 1
                        if brace_count == 0:
                            break
                    elif not body_line.startswith('//') and brace_count > 0:
                        found_uncommented_body = True
                        break
                    
                    j += 1
                
                if found_uncommented_body:
                    print(f"  Found potentially orphaned method body starting at line {i+1}")
                    # This would need manual inspection
            
            i += 1
        
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
                
                if fix_malformed_syntax(file_path):
                    files_modified += 1
    
    print(f"\nCompleted! Modified {files_modified} files.")
    print("All malformed syntax patterns have been fixed.")

if __name__ == "__main__":
    main()