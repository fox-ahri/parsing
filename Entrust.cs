namespace Template
{
    class Entrust
    {
        public delegate void UpdateStatus(WindowInfo info);
    }

    struct Params
    {
        public string input;
        public string output;
        public string error;
    };

    struct WindowInfo
    {
        public string name;
        public int process;
    };
}
