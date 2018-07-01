﻿namespace WindowsFormsApp1
{
    enum CmdLineType
    {
        Folder,
        File
    }
    class CMDFileFolder
    {
        private string _name;
        private string _size;
        private string _lastModificationData;
        private CmdLineType _type;

        public CMDFileFolder(CmdLineType type, string name, string size, string lastModificationData)
        {
            _type = type;
            _name = name;
            _size = size;
            _lastModificationData = lastModificationData;
        }

        public string GetName()
        {
            return _name;
        }

        public string getSize()
        {
            return _size;
        }

        public string GetLastModificationDate()
        {
            return _lastModificationData;
        }

        public new CmdLineType GetType()
        {
            return _type;
        }

        public override bool Equals(object obj)
        {
            var other = obj as CMDFileFolder;
            return other != null &&
                   other._type == _type &&
                   other._name == _name &&
                   other._size == _size &&
                   other._lastModificationData == _lastModificationData;
        }
    }
}
