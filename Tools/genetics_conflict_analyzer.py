#!/usr/bin/env python3
"""
Project Chimera: Genetics Namespace Conflict Analyzer
Identifies duplicate type definitions in the Genetics Data assembly
"""

import os
import re
from pathlib import Path
from typing import Dict, List, Set

class GeneticsConflictAnalyzer:
    def __init__(self, genetics_path: str):
        self.genetics_path = Path(genetics_path)
        self.type_definitions = {}  # type_name -> [(file, line_number, definition)]
        self.type_usages = {}       # type_name -> [(file, line_number, usage)]
        
    def analyze_conflicts(self):
        """Analyze all C# files for type conflicts"""
        print("Genetics Namespace Conflict Analysis")
        print("=" * 50)
        
        # Find all C# files
        cs_files = list(self.genetics_path.glob("*.cs"))
        print(f"Analyzing {len(cs_files)} C# files...")
        
        # Analyze each file
        for cs_file in cs_files:
            self.analyze_file(cs_file)
        
        # Find conflicts
        conflicts = self.find_conflicts()
        self.report_conflicts(conflicts)
        
        return conflicts
    
    def analyze_file(self, file_path: Path):
        """Analyze a single C# file for type definitions and usages"""
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                lines = f.readlines()
            
            for line_no, line in enumerate(lines, 1):
                # Look for class/struct/enum definitions
                class_match = re.search(r'^\s*public\s+(class|struct|enum)\s+(\w+)', line)
                if class_match:
                    type_kind, type_name = class_match.groups()
                    if type_name not in self.type_definitions:
                        self.type_definitions[type_name] = []
                    self.type_definitions[type_name].append({
                        'file': file_path.name,
                        'line': line_no,
                        'type': type_kind,
                        'definition': line.strip()
                    })
                
                # Look for type usages
                for type_name in ['BreedingChallenge', 'BreedingObjective', 'TargetTrait', 
                                'ScientificAchievement', 'SimpleTraitData', 'TournamentReward',
                                'GeneCategory']:
                    if type_name in line and not re.search(r'^\s*public\s+(class|struct|enum)\s+' + type_name, line):
                        if type_name not in self.type_usages:
                            self.type_usages[type_name] = []
                        self.type_usages[type_name].append({
                            'file': file_path.name,
                            'line': line_no,
                            'usage': line.strip()
                        })
        
        except Exception as e:
            print(f"Error analyzing {file_path}: {e}")
    
    def find_conflicts(self):
        """Find types that are defined in multiple files"""
        conflicts = {}
        
        for type_name, definitions in self.type_definitions.items():
            if len(definitions) > 1:
                conflicts[type_name] = {
                    'definitions': definitions,
                    'usages': self.type_usages.get(type_name, [])
                }
        
        return conflicts
    
    def report_conflicts(self, conflicts: Dict):
        """Report all conflicts found"""
        if not conflicts:
            print("✅ No conflicts found!")
            return
        
        print(f"\n❌ FOUND {len(conflicts)} TYPE CONFLICTS:")
        
        for type_name, conflict_data in conflicts.items():
            print(f"\n🔴 {type_name}:")
            print("  Definitions:")
            for defn in conflict_data['definitions']:
                print(f"    - {defn['file']}:{defn['line']} ({defn['type']}) - {defn['definition']}")
            
            if conflict_data['usages']:
                print("  Usages:")
                for usage in conflict_data['usages'][:5]:  # Show first 5 usages
                    print(f"    - {usage['file']}:{usage['line']}")
                if len(conflict_data['usages']) > 5:
                    print(f"    ... and {len(conflict_data['usages']) - 5} more")
        
        # Suggest resolution strategy
        print(f"\n💡 RESOLUTION STRATEGY:")
        for type_name, conflict_data in conflicts.items():
            canonical_file = self.suggest_canonical_file(type_name, conflict_data['definitions'])
            print(f"  {type_name}: Keep in {canonical_file}, remove from others")
    
    def suggest_canonical_file(self, type_name: str, definitions: List[Dict]) -> str:
        """Suggest which file should contain the canonical definition"""
        # Priority order for different types
        if type_name.startswith('Breeding'):
            # Breeding types should be in BreedingDataStructures or BreedingChallengeLibrarySO
            for defn in definitions:
                if 'BreedingDataStructures' in defn['file']:
                    return defn['file']
                if 'BreedingChallengeLibrarySO' in defn['file']:
                    return defn['file']
        
        if type_name.startswith('Scientific'):
            # Scientific types should be in ScientificDataStructures
            for defn in definitions:
                if 'ScientificDataStructures' in defn['file'] or 'ScientificAchievement' in defn['file']:
                    return defn['file']
        
        if type_name.startswith('Tournament'):
            # Tournament types should be in TournamentDataStructures
            for defn in definitions:
                if 'TournamentDataStructures' in defn['file'] or 'TournamentLibrary' in defn['file']:
                    return defn['file']
        
        # Default: prefer DataStructures files over SO files
        for defn in definitions:
            if 'DataStructures' in defn['file']:
                return defn['file']
        
        # Fallback: first definition
        return definitions[0]['file']

if __name__ == "__main__":
    genetics_path = "/Users/devon/Documents/Cursor/Projects/Assets/ProjectChimera/Data/Genetics"
    analyzer = GeneticsConflictAnalyzer(genetics_path)
    conflicts = analyzer.analyze_conflicts()