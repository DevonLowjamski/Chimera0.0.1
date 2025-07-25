#!/usr/bin/env python3
"""
Script to comment out manager usage (assignments and method calls) in UI files
to resolve CS0246 errors after breaking circular dependencies.
"""

import os
import re

def comment_manager_usage(file_path):
    """Comment out manager assignments and method calls in a file."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        lines = content.split('\n')
        modified = False
        
        for i, line in enumerate(lines):
            original_line = line
            
            # Pattern 1: Manager assignments like "_someManager = GameManager.Instance?.GetManager<SomeManager>();"
            assignment_pattern = r'^(\s*)(_\w*Manager\s*=.*)$'
            assignment_match = re.match(assignment_pattern, line)
            if assignment_match:
                indent, assignment = assignment_match.groups()
                lines[i] = f"{indent}// {assignment}"
                modified = True
                print(f"  Commented assignment: {assignment.strip()}")
                continue
            
            # Pattern 2: Manager method calls like "_someManager.SomeMethod();"
            method_call_pattern = r'^(\s*)(_\w*Manager\..*)$'
            method_call_match = re.match(method_call_pattern, line)
            if method_call_match:
                indent, method_call = method_call_match.groups()
                lines[i] = f"{indent}// {method_call}"
                modified = True
                print(f"  Commented method call: {method_call.strip()}")
                continue
            
            # Pattern 3: Lines containing manager usage in conditions like "if (_someManager == null)"
            condition_pattern = r'^(\s*)(.*_\w*Manager.*)$'
            condition_match = re.match(condition_pattern, line)
            if condition_match and ('if (' in line or 'while (' in line or 'return ' in line):
                indent, condition = condition_match.groups()
                # Only comment if it's not already commented
                if not condition.strip().startswith('//'):
                    lines[i] = f"{indent}// {condition}"
                    modified = True
                    print(f"  Commented condition: {condition.strip()}")
        
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
                
                if comment_manager_usage(file_path):
                    files_modified += 1
    
    print(f"\nCompleted! Modified {files_modified} files.")
    print("All manager assignments and method calls have been commented out.")

if __name__ == "__main__":
    main()