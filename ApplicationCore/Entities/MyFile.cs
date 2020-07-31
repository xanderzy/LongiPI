using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ApplicationCore.Entities
{
   public class MyFile:BaseEntity
    {
        [StringLength(int.MaxValue)]
        public string FilePath { set; get; }
        [StringLength(1024)]
        public string FileName { set; get; }
        [StringLength(8)]
        public string FileExt { set; get; }
        [StringLength(32)]
        public string FileIcon { set; get; }
        public int FileSize { set; get; }
        [StringLength(32)]
        public string FileMd5 { set; get; }
        public DateTime ModifyDt { set; get; }
        public DateTime CreateDt { set; get; }
        public int IsDelete { set; get; }
        public int TopicId { set; get; }
        public string Uploader { set; get; }
    }
}
