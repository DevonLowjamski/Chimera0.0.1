#!/usr/bin/env python3
"""
Project Chimera: Assembly Dependency Analyzer
Analyzes assembly dependencies to detect and visualize circular dependencies
"""

import json
import os
from pathlib import Path
from typing import Dict, List, Set, Tuple

class DependencyAnalyzer:
    def __init__(self, project_root: str):
        self.project_root = Path(project_root)
        self.assemblies = {}
        self.dependency_graph = {}
        
    def load_assemblies(self):
        """Load all assembly definitions"""
        asmdef_files = list(self.project_root.rglob("*.asmdef"))
        
        for asmdef_file in asmdef_files:
            try:
                with open(asmdef_file, 'r') as f:
                    data = json.load(f)
                    
                assembly_name = data.get("name", "")
                references = data.get("references", [])
                
                self.assemblies[assembly_name] = {
                    "file": str(asmdef_file.relative_to(self.project_root)),
                    "references": references
                }
                self.dependency_graph[assembly_name] = references
                
            except Exception as e:
                print(f"Error loading {asmdef_file}: {e}")
    
    def find_cycles(self) -> List[List[str]]:
        """Find all cycles in the dependency graph"""
        cycles = []
        visited = set()
        rec_stack = set()
        path = []
        
        def dfs(node: str) -> bool:
            if node in rec_stack:
                # Found a cycle, extract it from path
                cycle_start = path.index(node)
                cycle = path[cycle_start:] + [node]
                cycles.append(cycle)
                return True
                
            if node in visited:
                return False
                
            visited.add(node)
            rec_stack.add(node)
            path.append(node)
            
            for neighbor in self.dependency_graph.get(node, []):
                if neighbor in self.dependency_graph:  # Only follow known assemblies
                    if dfs(neighbor):
                        return True
            
            rec_stack.remove(node)
            path.pop()
            return False
        
        for assembly in self.dependency_graph:
            if assembly not in visited:
                dfs(assembly)
        
        return cycles
    
    def analyze_dependencies(self):
        """Analyze and report dependency issues"""
        print("Assembly Dependency Analysis")
        print("=" * 50)
        
        # Load assemblies
        self.load_assemblies()
        print(f"Loaded {len(self.assemblies)} assemblies")
        
        # Find cycles
        cycles = self.find_cycles()
        
        if cycles:
            print(f"\n❌ FOUND {len(cycles)} CIRCULAR DEPENDENCIES:")
            for i, cycle in enumerate(cycles, 1):
                print(f"\nCycle {i}: {' -> '.join(cycle)}")
        else:
            print("\n✅ No circular dependencies found")
        
        # Analyze dependency levels
        print("\n" + "=" * 50)
        print("DEPENDENCY BREAKDOWN BY ASSEMBLY:")
        
        for assembly, data in self.assemblies.items():
            refs = data["references"]
            print(f"\n{assembly}:")
            print(f"  File: {data['file']}")
            print(f"  Dependencies ({len(refs)}): {', '.join(refs) if refs else 'None'}")
        
        return cycles
    
    def suggest_fixes(self, cycles: List[List[str]]):
        """Suggest fixes for circular dependencies"""
        print("\n" + "=" * 50)
        print("SUGGESTED FIXES:")
        
        # Count how many cycles each assembly participates in
        cycle_participants = {}
        for cycle in cycles:
            for assembly in cycle:
                cycle_participants[assembly] = cycle_participants.get(assembly, 0) + 1
        
        # Sort by participation count (most problematic first)
        sorted_participants = sorted(cycle_participants.items(), key=lambda x: x[1], reverse=True)
        
        print("\nMost problematic assemblies (in most cycles):")
        for assembly, count in sorted_participants[:5]:
            print(f"  {assembly}: participates in {count} cycles")
        
        print("\nRecommended actions:")
        print("1. Remove dependencies from high-participation assemblies")
        print("2. Use event-driven communication instead of direct references")
        print("3. Consider creating interface assemblies for shared contracts")
        print("4. Move shared code to Core/Data assemblies")

if __name__ == "__main__":
    analyzer = DependencyAnalyzer(os.getcwd())
    cycles = analyzer.analyze_dependencies()
    if cycles:
        analyzer.suggest_fixes(cycles)