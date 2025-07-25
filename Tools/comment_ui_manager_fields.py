#!/usr/bin/env python3
"""
Script to comment out manager field declarations in UI files
to resolve CS0246 errors after breaking circular dependencies.
"""

import os
import re

def comment_manager_fields(file_path):
    """Comment out private manager field declarations in a file."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Pattern to match private manager field declarations
        # Matches: private SomeManager _someManager; or private SomeManager _someManager
        pattern = r'^(\s*)(private\s+\w*Manager\s+_\w*Manager[^;]*;?)(.*)$'
        
        lines = content.split('\n')
        modified = False
        
        for i, line in enumerate(lines):
            match = re.match(pattern, line)
            if match:
                indent, field_declaration, rest = match.groups()
                lines[i] = f"{indent}// {field_declaration}{rest}"
                modified = True
                print(f"  Commented: {field_declaration}")
        
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
                
                if comment_manager_fields(file_path):
                    files_modified += 1
    
    print(f"\nCompleted! Modified {files_modified} files.")
    print("All private manager field declarations have been commented out.")

if __name__ == "__main__":
    main()