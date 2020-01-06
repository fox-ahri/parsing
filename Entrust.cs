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
        public string template;
        public bool fragmentation;
        public string ruleFrag;
    };

    struct WindowInfo
    {
        public string name;
        public int process;
        public double success_count_lines;
        public double error_count_lines;
    };
}
