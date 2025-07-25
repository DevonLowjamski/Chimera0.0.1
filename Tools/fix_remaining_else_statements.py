#!/usr/bin/env python3
"""
Script to fix remaining orphaned else statements that follow commented closing braces.
"""

import os
import re

def fix_remaining_else_statements(file_path):
    """Fix else statements that follow commented closing braces."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        lines = content.split('\n')
        modified = False
        i = 0
        
        while i < len(lines):
            line = lines[i].strip()
            
            # Look for else statements
            if line == 'else' or line.startswith('else '):
                # Check if previous non-empty line is a commented closing brace
                prev_line_idx = i - 1
                while prev_line_idx >= 0 and lines[prev_line_idx].strip() == '':
                    prev_line_idx -= 1
                
                if prev_line_idx >= 0 and lines[prev_line_idx].strip() == '// }':
                    # This is an orphaned else after a commented closing brace
                    indent = len(lines[i]) - len(lines[i].lstrip())
                    lines[i] = ' ' * indent + '// ' + lines[i].lstrip()
                    modified = True
                    print(f"  Commented orphaned else at line {i+1}")
                    
                    # Also comment out the opening brace of the else block
                    if i + 1 < len(lines) and lines[i + 1].strip() == '{':
                        indent = len(lines[i + 1]) - len(lines[i + 1].lstrip())
                        lines[i + 1] = ' ' * indent + '// ' + lines[i + 1].lstrip()
                        print(f"  Commented orphaned else opening brace at line {i+2}")
                        
                        # Find and comment the matching closing brace
                        brace_count = 1
                        j = i + 2
                        while j < len(lines) and brace_count > 0:
                            if '{' in lines[j] and not lines[j].strip().startswith('//'):
                                brace_count += lines[j].count('{')
                            if '}' in lines[j] and not lines[j].strip().startswith('//'):
                                brace_count -= lines[j].count('}')
                                if brace_count == 0:
                                    indent = len(lines[j]) - len(lines[j].lstrip())
                                    lines[j] = ' ' * indent + '// ' + lines[j].lstrip()
                                    print(f"  Commented orphaned else closing brace at line {j+1}")
                                    break
                            j += 1
            
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
                
                if fix_remaining_else_statements(file_path):
                    files_modified += 1
    
    print(f"\nCompleted! Modified {files_modified} files.")
    print("All remaining orphaned else statements have been fixed.")

if __name__ == "__main__":
    main()