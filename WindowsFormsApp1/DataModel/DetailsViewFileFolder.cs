﻿using System.Collections.Generic;
using WindowsFormsApp1.DataModel;

namespace WindowsFormsApp1
{
    public class DetailsViewFileFolder : Showable
    {
        private string _name;
        private string _size;
        private string _lastModificationData;
        private FileFolderImageType _type;

        public DetailsViewFileFolder(FileFolderImageType type, string name, string size, string lastModificationData) : base(type.ToString(),
                                                                                                             new List<string>() {name,
                                                                                                                                 type == FileFolderImageType.Folder ?  string.Empty : size,
                                                                                                                                  lastModificationData},
                                                                                                             (int)type)

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

        public new FileFolderImageType GetType()
        {
            return _type;
        }

        public override bool Equals(object obj)
        {
            var other = obj as DetailsViewFileFolder;
            return other != null &&
                   other._type == _type &&
                   other._name == _name &&
                   other._size == _size &&
                   other._lastModificationData == _lastModificationData;
        }
    }
}
