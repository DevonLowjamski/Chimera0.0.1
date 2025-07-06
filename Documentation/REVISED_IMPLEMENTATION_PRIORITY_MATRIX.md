# Project Chimera - Revised Implementation Priority Matrix

**Document Version:** 1.1 (Aligned with Architectural Analysis)  
**Date:** January 2025  
**Priority Revision:** Based on Architectural Compendium Analysis  

---

## 🎯 **CRITICAL PRIORITY REVISIONS**

### **Phase 0: Foundation Prerequisites (Weeks 1-2)**
**NEW HIGHEST PRIORITY** - Based on Architectural Document Emphasis

#### **0.1 Chimera Debug Inspector Implementation** 
🔥 **CRITICAL** - "Single most critical prerequisite for all future work"
- **Week 1**: Create UI Toolkit document (`DebugInspector.uxml`)
- **Week 1**: Implement `DebugInspector.cs` with real-time plant data visualization
- **Week 1**: Integration with `DataManager` for `PlantInstanceSO` access
- **Week 2**: Time control buttons (Pause, Play, Fast-Forward) via `TimeManager`
- **Week 2**: Genotype/phenotype display with live trait expression data

#### **0.2 Core Genetic Engine - Phase 1** 
🔥 **CRITICAL** - Enable GxE simulation foundation
- **Week 2**: Implement `TraitExpressionEngine.CalculateExpression()` for single trait (Height)
- **Week 2**: Refactor `PlantInstanceSO.ProcessDailyGrowth()` to use trait expression
- **Week 2**: Create test scene with three plants (HH, Hh, hh genotypes)

---

## 🧬 **Phase 1: Genetic Engine Completion (Weeks 3-6)**

### **1.1 Breeding System Implementation**
- **Week 3**: Complete `BreedingSimulator.PerformBreeding()` with Mendelian genetics
- **Week 3**: Create breeding UI for parent selection and offspring generation
- **Week 4**: Verify genetic inheritance through Debug Inspector

### **1.2 Advanced Trait Expression**
- **Week 4**: Expand TraitExpressionEngine for multiple traits (THC, CBD, Yield)
- **Week 5**: Implement pleiotropy and epistasis systems
- **Week 5**: Environmental modification of trait expression (GxE interactions)

### **1.3 UI Data Binding Integration**
- **Week 6**: Connect `PlantManagementPanel` to live `PlantInstanceSO` data
- **Week 6**: Implement event-driven UI updates for plant state changes
- **Week 6**: Complete data binding for all cultivation UI panels

---

## 🌿 **Phase 2: VFX & Visual Systems (Weeks 7-10)**

### **2.1 VFX Graph Package & Cannabis Effects**
- **Week 7**: Install VFX Graph package and configure cannabis-specific effects
- **Week 8**: Implement trichrome development, growth transitions, health visualization
- **Week 9**: SpeedTree-VFX integration with 11 attachment points

### **2.2 SpeedTree Enhancements**
- **Week 10**: Advanced LOD system, genetic morphology integration
- **Week 10**: Performance optimization for large-scale cultivation

---

## 📊 **Phase 3: System Completion (Weeks 11-16)**

### **3.1 Construction & IPM Systems**
- **Weeks 11-12**: Complete construction system implementation
- **Weeks 13-14**: Enable IPM system components and integration

### **3.2 Performance & Polish**
- **Weeks 15-16**: System-wide performance optimization
- **Week 16**: Final integration testing and documentation

---

## 🔍 **ARCHITECTURAL ALIGNMENT VERIFICATION**

### ✅ **Addresses All Architectural Concerns:**
1. **Debug Inspector** - Now Phase 0 (Critical Priority)
2. **Genetic Engine** - Core focus of Phases 0-1
3. **UI Data Binding** - Phase 1.3 implementation
4. **VFX Integration** - Phase 2.1 comprehensive setup
5. **SpeedTree Enhancement** - Phase 2.2 advanced features

### 📈 **Implementation Success Metrics:**
- **Week 2**: Debug Inspector functional with real-time plant data
- **Week 4**: Genetic breeding system producing viable offspring
- **Week 6**: UI panels displaying live simulation data
- **Week 10**: VFX-enhanced cannabis growth visualization
- **Week 16**: Production-ready cultivation simulation

---

## 🎮 **DEVELOPMENT MILESTONES**

**Milestone 0.3**: Debug Inspector & Genetic Foundation (Week 2)
**Milestone 1.1**: Functional Breeding System (Week 4)  
**Milestone 1.2**: Complete GxE Simulation (Week 6)
**Milestone 2.1**: VFX-Enhanced Cultivation (Week 10)
**Milestone 3.0**: Production-Ready System (Week 16)

This revised priority matrix ensures that Project Chimera's implementation follows the exact sequence recommended by the architectural analysis, prioritizing the Debug Inspector as the foundation for all subsequent genetic engine development.
