// Test file to verify genetics namespace can be imported
using ProjectChimera.Data.Genetics;

namespace ProjectChimera.Data
{
    public class GeneticsUseTest
    {
        public GeneticsTest testField;
        
        public void TestMethod()
        {
            // This should compile if namespace import is working
            testField = new GeneticsTest();
        }
    }
}