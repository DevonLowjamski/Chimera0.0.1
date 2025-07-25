#!/usr/bin/env python3
"""
Analyze circular dependencies in Unity assembly definition files.
"""

import json
import os
from pathlib import Path
from collections import defaultdict, deque

def find_asmdef_files(root_path):
    """Find all .asmdef files in the project."""
    asmdef_files = []
    for root, dirs, files in os.walk(root_path):
        for file in files:
            if file.endswith('.asmdef'):
                asmdef_files.append(os.path.join(root, file))
    return asmdef_files

def parse_asmdef(file_path):
    """Parse an assembly definition file."""
    try:
        with open(file_path, 'r') as f:
            data = json.load(f)
        return {
            'name': data.get('name', ''),
            'references': data.get('references', []),
            'path': file_path
        }
    except Exception as e:
        print(f"Error parsing {file_path}: {e}")
        return None

def build_dependency_graph(asmdef_files):
    """Build a dependency graph from assembly definitions."""
    assemblies = {}
    graph = defaultdict(list)
    
    # First pass: collect all assemblies
    for file_path in asmdef_files:
        asmdef = parse_asmdef(file_path)
        if asmdef:
            assemblies[asmdef['name']] = asmdef
    
    # Second pass: build dependency graph
    for name, asmdef in assemblies.items():
        for ref in asmdef['references']:
            if ref in assemblies:  # Only include references to assemblies in our project
                graph[name].append(ref)
    
    return assemblies, graph

def find_strongly_connected_components(graph):
    """Find strongly connected components using Tarjan's algorithm."""
    index_counter = [0]
    stack = []
    lowlinks = {}
    index = {}
    on_stack = {}
    components = []
    
    def strongconnect(node):
        index[node] = index_counter[0]
        lowlinks[node] = index_counter[0]
        index_counter[0] += 1
        stack.append(node)
        on_stack[node] = True
        
        for successor in graph.get(node, []):
            if successor not in index:
                strongconnect(successor)
                lowlinks[node] = min(lowlinks[node], lowlinks[successor])
            elif on_stack.get(successor, False):
                lowlinks[node] = min(lowlinks[node], index[successor])
        
        if lowlinks[node] == index[node]:
            component = []
            while True:
                w = stack.pop()
                on_stack[w] = False
                component.append(w)
                if w == node:
                    break
            if len(component) > 1:  # Only consider components with more than one node
                components.append(component)
    
    for node in graph:
        if node not in index:
            strongconnect(node)
    
    return components

def analyze_dependencies():
    """Main analysis function."""
    project_root = "/Users/devon/Documents/Cursor/Projects/Assets"
    
    print("🔍 Analyzing Unity Assembly Dependencies...")
    print("=" * 60)
    
    # Find all assembly definition files
    asmdef_files = find_asmdef_files(project_root)
    print(f"Found {len(asmdef_files)} assembly definition files")
    
    # Build dependency graph
    assemblies, graph = build_dependency_graph(asmdef_files)
    print(f"Analyzing {len(assemblies)} assemblies")
    
    # Find circular dependencies
    circular_deps = find_strongly_connected_components(graph)
    
    print("\n📊 Analysis Results:")
    print("-" * 40)
    
    if circular_deps:
        print(f"❌ Found {len(circular_deps)} circular dependency group(s):")
        
        for i, component in enumerate(circular_deps, 1):
            print(f"\n🔄 Circular Dependency Group {i}:")
            for assembly in component:
                print(f"  - {assembly}")
                if assembly in graph:
                    for ref in graph[assembly]:
                        if ref in component:
                            print(f"    → {ref}")
            
            print(f"\n💡 To fix this circular dependency:")
            print("   1. Identify which assemblies should depend on others")
            print("   2. Remove references that create the cycle")
            print("   3. Use event-driven communication instead of direct references")
            
    else:
        print("✅ No circular dependencies found!")
    
    # Print all assembly references for debugging
    print(f"\n📋 All Assembly References:")
    print("-" * 40)
    
    for name, asmdef in sorted(assemblies.items()):
        print(f"\n{name}:")
        if asmdef['references']:
            for ref in sorted(asmdef['references']):
                status = "✅" if ref in assemblies else "❌"
                print(f"  {status} {ref}")
        else:
            print("  (no references)")
    
    return len(circular_deps) == 0

if __name__ == "__main__":
    success = analyze_dependencies()
    exit(0 if success else 1)