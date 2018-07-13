namespace WindowsFormsApp1
{
    public class FileFolder
    {
        private string _name;
        private string _size;
        private string _lastModificationData;
        private FileFolderType _type;

        public FileFolder(FileFolderType type, string name, string size, string lastModificationData)
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

        public new FileFolderType GetType()
        {
            return _type;
        }

        public override bool Equals(object obj)
        {
            var other = obj as FileFolder;
            return other != null &&
                   other._type == _type &&
                   other._name == _name &&
                   other._size == _size &&
                   other._lastModificationData == _lastModificationData;
        }
    }
}
