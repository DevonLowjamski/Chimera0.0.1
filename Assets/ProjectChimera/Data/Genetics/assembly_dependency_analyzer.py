#!/usr/bin/env python3
"""
Unity Assembly Definition Circular Dependency Analyzer
Analyzes .asmdef files to detect circular dependencies between Unity assemblies.
"""

import json
import os
import sys
from collections import defaultdict, deque
from pathlib import Path
from typing import Dict, List, Set, Tuple, Optional

class AssemblyDependencyAnalyzer:
    def __init__(self, project_root: str):
        self.project_root = Path(project_root)
        self.assemblies: Dict[str, dict] = {}
        self.dependency_graph: Dict[str, Set[str]] = defaultdict(set)
        self.reverse_dependency_graph: Dict[str, Set[str]] = defaultdict(set)
        
    def find_asmdef_files(self) -> List[Path]:
        """Find all .asmdef files in the project."""
        asmdef_files = []
        for root, dirs, files in os.walk(self.project_root):
            for file in files:
                if file.endswith('.asmdef'):
                    asmdef_files.append(Path(root) / file)
        return asmdef_files
    
    def load_assembly_definitions(self) -> None:
        """Load and parse all assembly definition files."""
        asmdef_files = self.find_asmdef_files()
        print(f"Found {len(asmdef_files)} .asmdef files")
        
        for asmdef_file in asmdef_files:
            try:
                with open(asmdef_file, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                    assembly_name = data.get('name', asmdef_file.stem)
                    self.assemblies[assembly_name] = {
                        'file_path': asmdef_file,
                        'data': data,
                        'references': data.get('references', []),
                        'assembly_references': data.get('assemblyReferences', [])
                    }
                    print(f"Loaded: {assembly_name}")
            except Exception as e:
                print(f"Error loading {asmdef_file}: {e}")
    
    def build_dependency_graph(self) -> None:
        """Build the dependency graph from assembly definitions."""
        for assembly_name, assembly_info in self.assemblies.items():
            # Add references (direct assembly dependencies)
            for ref in assembly_info['references']:
                self.dependency_graph[assembly_name].add(ref)
                self.reverse_dependency_graph[ref].add(assembly_name)
            
            # Add assembly references (external DLL dependencies)
            for ref in assembly_info['assembly_references']:
                # Extract assembly name from GUID or direct reference
                if ref.startswith('GUID:'):
                    # For GUID references, we'd need to resolve them, but for now skip
                    continue
                else:
                    self.dependency_graph[assembly_name].add(ref)
                    self.reverse_dependency_graph[ref].add(assembly_name)
    
    def detect_circular_dependencies(self) -> List[List[str]]:
        """Detect all circular dependencies using DFS."""
        visited = set()
        rec_stack = set()
        cycles = []
        
        def dfs(node: str, path: List[str]) -> None:
            if node in rec_stack:
                # Found a cycle
                cycle_start = path.index(node)
                cycle = path[cycle_start:] + [node]
                cycles.append(cycle)
                return
            
            if node in visited:
                return
            
            visited.add(node)
            rec_stack.add(node)
            path.append(node)
            
            for neighbor in self.dependency_graph.get(node, set()):
                if neighbor in self.assemblies:  # Only follow internal assemblies
                    dfs(neighbor, path.copy())
            
            rec_stack.remove(node)
        
        # Start DFS from each unvisited node
        for assembly in self.assemblies.keys():
            if assembly not in visited:
                dfs(assembly, [])
        
        return cycles
    
    def find_strongly_connected_components(self) -> List[List[str]]:
        """Find strongly connected components using Tarjan's algorithm."""
        index_counter = [0]
        stack = []
        lowlinks = {}
        index = {}
        on_stack = {}
        scc_list = []
        
        def strongconnect(node):
            index[node] = index_counter[0]
            lowlinks[node] = index_counter[0]
            index_counter[0] += 1
            stack.append(node)
            on_stack[node] = True
            
            for neighbor in self.dependency_graph.get(node, set()):
                if neighbor not in self.assemblies:  # Skip external assemblies
                    continue
                    
                if neighbor not in index:
                    strongconnect(neighbor)
                    lowlinks[node] = min(lowlinks[node], lowlinks[neighbor])
                elif on_stack.get(neighbor, False):
                    lowlinks[node] = min(lowlinks[node], index[neighbor])
            
            if lowlinks[node] == index[node]:
                component = []
                while True:
                    w = stack.pop()
                    on_stack[w] = False
                    component.append(w)
                    if w == node:
                        break
                if len(component) > 1:  # Only report SCCs with multiple nodes
                    scc_list.append(component)
        
        for assembly in self.assemblies.keys():
            if assembly not in index:
                strongconnect(assembly)
        
        return scc_list
    
    def analyze_dependency_violations(self) -> Dict[str, List[str]]:
        """Analyze common dependency violations."""
        violations = {
            'editor_runtime_cycles': [],
            'test_production_cycles': [],
            'circular_core_dependencies': [],
            'missing_dependencies': []
        }
        
        # Check for Editor assemblies depending on runtime assemblies that depend back
        for assembly_name, assembly_info in self.assemblies.items():
            if '.Editor' in assembly_name:
                for dep in assembly_info['references']:
                    if dep in self.assemblies and '.Editor' not in dep:
                        # Runtime assembly that Editor depends on
                        if assembly_name in self.dependency_graph.get(dep, set()):
                            violations['editor_runtime_cycles'].append(f"{assembly_name} <-> {dep}")
            
            if '.Testing' in assembly_name or '.Test' in assembly_name:
                for dep in assembly_info['references']:
                    if dep in self.assemblies and '.Testing' not in dep and '.Test' not in dep:
                        if assembly_name in self.dependency_graph.get(dep, set()):
                            violations['test_production_cycles'].append(f"{assembly_name} <-> {dep}")
        
        return violations
    
    def generate_fix_recommendations(self, cycles: List[List[str]], violations: Dict[str, List[str]]) -> List[str]:
        """Generate specific recommendations to fix circular dependencies."""
        recommendations = []
        
        recommendations.append("=== CIRCULAR DEPENDENCY FIX RECOMMENDATIONS ===\n")
        
        if cycles:
            recommendations.append("1. DETECTED CIRCULAR DEPENDENCY CYCLES:")
            for i, cycle in enumerate(cycles, 1):
                recommendations.append(f"\n   Cycle {i}: {' -> '.join(cycle)}")
                
                # Analyze the cycle to suggest fixes
                core_assemblies = [a for a in cycle if 'Core' in a]
                editor_assemblies = [a for a in cycle if 'Editor' in a]
                test_assemblies = [a for a in cycle if 'Testing' in a or 'Test' in a]
                
                recommendations.append(f"   Analysis:")
                if core_assemblies:
                    recommendations.append(f"     - Core assemblies in cycle: {core_assemblies}")
                if editor_assemblies:
                    recommendations.append(f"     - Editor assemblies in cycle: {editor_assemblies}")
                if test_assemblies:
                    recommendations.append(f"     - Test assemblies in cycle: {test_assemblies}")
                
                recommendations.append(f"   Suggested fixes:")
                
                # Common fix patterns
                if editor_assemblies and len([a for a in cycle if 'Editor' not in a]) > 0:
                    recommendations.append(f"     - Move shared code between Editor and Runtime to a separate shared assembly")
                    recommendations.append(f"     - Use interfaces and dependency injection instead of direct references")
                
                if test_assemblies:
                    recommendations.append(f"     - Test assemblies should only reference production code, never the reverse")
                    recommendations.append(f"     - Move test utilities to a separate TestUtilities assembly")
                
                if 'Assembly-CSharp' in cycle:
                    recommendations.append(f"     - Move code out of Assembly-CSharp into specific assemblies")
                    recommendations.append(f"     - Assembly-CSharp should be minimal and only contain scene-specific code")
        
        recommendations.append("\n2. SPECIFIC VIOLATIONS DETECTED:")
        for violation_type, items in violations.items():
            if items:
                recommendations.append(f"\n   {violation_type.replace('_', ' ').title()}:")
                for item in items:
                    recommendations.append(f"     - {item}")
        
        recommendations.append("\n3. GENERAL RECOMMENDATIONS:")
        recommendations.append("   - Follow dependency hierarchy: Core <- Data <- Systems <- UI <- Testing")
        recommendations.append("   - Use interfaces and events for loose coupling")
        recommendations.append("   - Move shared code to lower-level assemblies")
        recommendations.append("   - Keep Assembly-CSharp minimal")
        recommendations.append("   - Editor assemblies should not be referenced by runtime assemblies")
        recommendations.append("   - Test assemblies should only reference production assemblies")
        
        return recommendations
    
    def export_dependency_graph(self, output_file: str) -> None:
        """Export dependency graph to DOT format for visualization."""
        with open(output_file, 'w') as f:
            f.write("digraph AssemblyDependencies {\n")
            f.write("  rankdir=TB;\n")
            f.write("  node [shape=box];\n\n")
            
            # Color code different types of assemblies
            for assembly in self.assemblies.keys():
                color = "lightblue"
                if "Editor" in assembly:
                    color = "lightcoral"
                elif "Testing" in assembly or "Test" in assembly:
                    color = "lightgreen"
                elif "Core" in assembly:
                    color = "gold"
                
                f.write(f'  "{assembly}" [fillcolor={color}, style=filled];\n')
            
            f.write("\n")
            
            # Write edges
            for assembly, deps in self.dependency_graph.items():
                for dep in deps:
                    if dep in self.assemblies:  # Only show internal dependencies
                        f.write(f'  "{assembly}" -> "{dep}";\n')
            
            f.write("}\n")
        
        print(f"Dependency graph exported to {output_file}")
        print("Use Graphviz to visualize: dot -Tpng dependency_graph.dot -o dependency_graph.png")
    
    def run_analysis(self) -> None:
        """Run the complete analysis."""
        print("Starting Assembly Dependency Analysis...")
        print("=" * 50)
        
        # Load assembly definitions
        self.load_assembly_definitions()
        print(f"\nLoaded {len(self.assemblies)} assemblies")
        
        # Build dependency graph
        self.build_dependency_graph()
        print("Built dependency graph")
        
        # Print assembly summary
        print(f"\nASSEMBLY SUMMARY:")
        print("-" * 30)
        for assembly_name, assembly_info in sorted(self.assemblies.items()):
            refs = assembly_info['references']
            print(f"{assembly_name}: {len(refs)} references")
            if refs:
                print(f"  -> {', '.join(refs)}")
        
        # Detect cycles
        print(f"\nDETECTING CIRCULAR DEPENDENCIES...")
        print("-" * 40)
        
        scc_list = self.find_strongly_connected_components()
        cycles = self.detect_circular_dependencies()
        violations = self.analyze_dependency_violations()
        
        if scc_list:
            print(f"Found {len(scc_list)} strongly connected components (circular dependency groups):")
            for i, scc in enumerate(scc_list, 1):
                print(f"\nSCC {i}: {' <-> '.join(scc)}")
        else:
            print("No strongly connected components found!")
        
        if cycles:
            print(f"\nFound {len(cycles)} dependency cycles:")
            for i, cycle in enumerate(cycles, 1):
                print(f"Cycle {i}: {' -> '.join(cycle)}")
        else:
            print("No dependency cycles detected!")
        
        # Generate recommendations
        recommendations = self.generate_fix_recommendations(scc_list, violations)
        
        print(f"\n" + "\n".join(recommendations))
        
        # Export graph
        graph_file = self.project_root / "assembly_dependency_graph.dot"
        self.export_dependency_graph(str(graph_file))
        
        # Save detailed report
        report_file = self.project_root / "assembly_dependency_report.txt"
        with open(report_file, 'w') as f:
            f.write("Unity Assembly Dependency Analysis Report\n")
            f.write("=" * 50 + "\n\n")
            
            f.write(f"Total Assemblies: {len(self.assemblies)}\n")
            f.write(f"Strongly Connected Components: {len(scc_list)}\n")
            f.write(f"Dependency Cycles: {len(cycles)}\n\n")
            
            f.write("ASSEMBLY DETAILS:\n")
            f.write("-" * 20 + "\n")
            for assembly_name, assembly_info in sorted(self.assemblies.items()):
                f.write(f"\n{assembly_name}:\n")
                f.write(f"  File: {assembly_info['file_path']}\n")
                f.write(f"  References: {assembly_info['references']}\n")
            
            if scc_list:
                f.write(f"\n\nSTRONGLY CONNECTED COMPONENTS:\n")
                f.write("-" * 35 + "\n")
                for i, scc in enumerate(scc_list, 1):
                    f.write(f"\nSCC {i}: {' <-> '.join(scc)}\n")
            
            if cycles:
                f.write(f"\n\nDEPENDENCY CYCLES:\n")
                f.write("-" * 20 + "\n")
                for i, cycle in enumerate(cycles, 1):
                    f.write(f"\nCycle {i}: {' -> '.join(cycle)}\n")
            
            f.write(f"\n\n" + "\n".join(recommendations))
        
        print(f"\nDetailed report saved to: {report_file}")
        print(f"Analysis complete!")

def main():
    if len(sys.argv) > 1:
        project_root = sys.argv[1]
    else:
        project_root = "/Users/devon/Documents/Cursor/Projects/Assets"
    
    analyzer = AssemblyDependencyAnalyzer(project_root)
    analyzer.run_analysis()

if __name__ == "__main__":
    main()