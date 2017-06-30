# Source dependency BP check for Dynamics 365 Unified Operations platform

When creating extensions for Dynamics 365 Unified Operations applications (Finance and Operations, Retail), you may take
a dependency on what the original Microsoft source code does. For example, pre/post method handlers, or because of using the time
honored "copy and paste" approach, or you just want a warning if a main processing flow changes.

To assist developers in being notified if a change has happened in a method they depend on, a small BP check was created together
with an X++ attribute `AdSourceDependency` with which you decorate the methods having a dependency. For example:

    class TestClass1
    {
        void blabla()
        {
            info('krnekiii');
        }

        [AdSourceDependency(classStr(TestClass1), methodStr(TestClass1, blabla), literalStr('6daac6f7d1e91ef62dfe6fc0fd93455'))]
        void bla2()
        {
            this.blabla();
        }
    }

This will throw a BP error because I have changed the original text of blabla():

    SourceDependencyChanged: BP Rule: [SourceDependencyChanged]:Dependent method has changed - expected hash 6daac6f7d1e91ef62dfe6fc0fd93455, calculated hash 86daac6f7d1e91ef62dfe6fc0fd93455

After reviewing the changes to the method, you update the hash and the BP error goes away.

You need to use intrinsic methods (classStr, tableStr, methodStr, tableMethodStr, and literalStr for the hash) in the parameters to  `AdSourceDependency`, so the check knows which type to check. Currently, classes and tables are supported.
