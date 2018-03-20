using System
;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Himall.Core;
using Aliyun.OSS;
using System.IO;

namespace Himall.Strategy
{
	public class OSS : IHimallIO
	{
		private static IOss ossClient = null;

		public OSS()
		{
			ossClient = OssClientFactory.CreateOssClient();
		}

		public string GetFilePath(string fileName)
		{
			fileName = GetFileName(fileName);
			if (!string.IsNullOrEmpty(fileName))
			{
				return string.Format("http://{0}/{1}", Config.FileServerDomain, fileName);
			}
			return fileName;
		}

		public string GetProductSizeImage(string productPath, int index, int width = 0)
		{
			if (string.IsNullOrWhiteSpace(productPath))
			{
				return string.Empty;
			}
			var image = productPath;
            if(image.IndexOf("?")>0)
            {
                image = image.Substring(0, image.IndexOf('?'));
            }
			if (string.IsNullOrEmpty(Path.GetExtension(image)) && !image.EndsWith("/"))
			{
				image = image + "/";
			}
			image = Path.GetDirectoryName(image).Replace("\\", "/");
			if (string.IsNullOrWhiteSpace(Config.ImageServerDomain))
				throw new HimallIOException("调用获取图片路径接口必须配置ImageServerDomain节点");
			var imageName = string.Format("{0}/{1}.png", image, index);
			if (!string.IsNullOrEmpty(imageName))
			{
				string url = string.Empty;
				if (width == 0)
				{
					if (imageName.StartsWith("http"))
					{
						url = imageName.Replace("http:/", "http://");
					}
					else
					{
						url = string.Format("http://{0}/{1}", Config.ImageServerDomain, imageName);
					}
				}

				if (width != 0)
				{
					string styleSplitChars = "?x-oss-process=image/resize,w_";//OSS默认样式分隔符
					if (imageName.StartsWith("http"))
					{
						imageName = imageName.Replace("http:/", "http://");
						url = string.Format("{0}{1}{2}", imageName, styleSplitChars, width);
					}
					else
					{
						url = string.Format("http://{0}/{1}{2}{3}", Config.ImageServerDomain, imageName, styleSplitChars, width);
					}
				}
				return url;
			}
			else
			{
				return imageName;
			}
		}


		public string GetImagePath(string imageName, string styleName = null)
		{
			if (string.IsNullOrWhiteSpace(Config.ImageServerDomain))
				throw new HimallIOException("调用获取图片路径接口必须配置ImageServerDomain节点");
			imageName = GetFileName(imageName);
			if (string.IsNullOrEmpty(imageName))
			{
				return imageName;
			}
			if (imageName.StartsWith("http"))
			{
				return imageName;
			}
			if (!string.IsNullOrEmpty(imageName))
			{
				string url = string.Empty;
				if (string.IsNullOrWhiteSpace(styleName))
					url = string.Format("http://{0}/{1}", Config.ImageServerDomain, imageName);
				else
				{
                    //string styleSplitChars = "@!";//OSS默认样式分隔符
                    string styleSplitChars = "?x-oss-process=style/";

                    url = string.Format("http://{0}/{1}{2}{3}", Config.ImageServerDomain, imageName, styleSplitChars, styleName);
				}
				return url;
			}
			else
			{
				return imageName;
			}
		}

		public byte[] GetFileContent(string fileName)
		{
			if (ExistFile(fileName))
			{
				fileName = GetFileName(fileName);
				var result = ossClient.GetObject(Config.BucketName, fileName);
				List<byte> bytes = new List<byte>();
				if (result.Content != null)
				{
					int temp = result.Content.ReadByte();
					while (temp != -1)
					{
						bytes.Add((byte)temp);
						temp = result.Content.ReadByte();
					}
				}
				return bytes.ToArray();
			}
			else
			{
				throw new HimallIOException(IOErrorMsg.FileNotExist.ToDescription());
			}
		}

		public void CreateFile(string fileName, Stream stream, FileCreateType fileCreateType = FileCreateType.CreateNew)
		{
			fileName = GetFileName(fileName);
			if (fileCreateType == FileCreateType.CreateNew)
			{
				if (!ExistFile(fileName))
				{
					RecurseCreateFileDir(fileName);

					ossClient.PutObject(Config.BucketName, fileName, stream);
				}
				else
				{
					throw new HimallIOException(IOErrorMsg.FileExist.ToDescription());
				}
			}
			else
			{
				RecurseCreateFileDir(fileName);

				ossClient.PutObject(Config.BucketName, fileName, stream);
			}
		}


		public void CreateFile(string fileName, string content, FileCreateType fileCreateType = FileCreateType.CreateNew)
		{
			fileName = GetFileName(fileName);
			if (fileCreateType == FileCreateType.CreateNew)
			{
				if (!ExistFile(fileName))
				{
					RecurseCreateFileDir(fileName);

					byte[] binaryData = Encoding.UTF8.GetBytes(content);
					using (var stream = new MemoryStream(binaryData))
					{
						ossClient.PutObject(Config.BucketName, fileName, stream);
					}
				}
				else
				{
					throw new HimallIOException(IOErrorMsg.FileExist.ToDescription());
				}
			}
			else
			{
				RecurseCreateFileDir(fileName);

				byte[] binaryData = Encoding.UTF8.GetBytes(content);
				var stream = new MemoryStream(binaryData);
				ossClient.PutObject(Config.BucketName, fileName, stream);
			}
		}

		public void CreateDir(string dirName)
		{
			dirName = GetDirName(dirName);
			if (!ExistDir(dirName))
			{
				var dirs = dirName.Remove(dirName.Length - 1).Split('/').ToList();

				RecurseCreateDir(dirs);
			}
		}

		public bool ExistFile(string fileName)
		{
			fileName = GetFileName(fileName);
			var result = ossClient.DoesObjectExist(Config.BucketName, fileName);
			return result;
		}

		public bool ExistDir(string dirName)
		{
			dirName = GetDirName(dirName);
			var result = ossClient.DoesObjectExist(Config.BucketName, dirName);
			return result;
		}

		public void DeleteDir(string dirName, bool recursive = false)
		{
			dirName = GetDirName(dirName);
			if (ExistDir(dirName))
			{
				if (recursive)
				{
					var keys = GetFiles(dirName, true);
					var request = new DeleteObjectsRequest(Config.BucketName, keys);
					ossClient.DeleteObjects(request);
				}
				else
				{
					var files = GetDirAndFiles(dirName);
					if (files == null || files.Count == 0)
					{
						ossClient.DeleteObject(Config.BucketName, dirName);
					}
					else
					{
						throw new HimallIOException(IOErrorMsg.DirDeleleError.ToDescription());
					}
				}
			}
		}

		public void DeleteFile(string fileName)
		{
			fileName = GetFileName(fileName);
			if (ExistFile(fileName))
			{
				ossClient.DeleteObject(Config.BucketName, fileName);
			}
		}

		public void DeleteFiles(List<string> fileNames)
		{
			var request = new DeleteObjectsRequest(Config.BucketName, fileNames, false);
			ossClient.DeleteObjects(request);
		}

		public void CopyFile(string sourceFileName, string destFileName, bool overwrite = false)
		{
			sourceFileName = GetFileName(sourceFileName);
			if (ExistFile(sourceFileName))
			{
				var metadata = ossClient.GetObjectMetadata(Config.BucketName, sourceFileName);
				if (metadata.ObjectType == "Appendable")
				{
					throw new HimallIOException(IOErrorMsg.AppendFileNotOperate.ToDescription());
				}

				destFileName = GetFileName(destFileName);
				if (!overwrite && ExistFile(destFileName)) //如果不可覆盖且目标文件已存在
				{
					throw new HimallIOException(IOErrorMsg.FileExist.ToDescription());
				}
				else
				{
					RecurseCreateFileDir(destFileName);
				}

				var req = new CopyObjectRequest(Config.BucketName, sourceFileName, Config.BucketName, destFileName);
				ossClient.CopyObject(req);
			}
			else
			{
				throw new HimallIOException(IOErrorMsg.FileNotExist.ToDescription());
			}
		}

		public void MoveFile(string sourceFileName, string destFileName, bool overwrite = false)
		{
			sourceFileName = GetFileName(sourceFileName);
			if (ExistFile(sourceFileName))
			{
				var metadata = ossClient.GetObjectMetadata(Config.BucketName, sourceFileName);
				if (metadata.ObjectType == "Appendable")
				{
					throw new HimallIOException(IOErrorMsg.AppendFileNotOperate.ToDescription());
				}

				destFileName = GetFileName(destFileName);
				if (!overwrite && ExistFile(destFileName)) //如果不可覆盖且目标文件已存在
				{
					throw new HimallIOException(IOErrorMsg.FileExist.ToDescription());
				}
				else
				{
					RecurseCreateFileDir(destFileName);
				}

				var req = new CopyObjectRequest(Config.BucketName, sourceFileName, Config.BucketName, destFileName);
				ossClient.CopyObject(req);
				ossClient.DeleteObject(Config.BucketName, sourceFileName);
			}
			else
			{
				throw new HimallIOException(IOErrorMsg.FileNotExist.ToDescription());
			}
		}

		public List<string> GetDirAndFiles(string dirName, bool self = false)
		{
			dirName = GetDirName(dirName);
			var keys = new List<string>();
			var listObjectsRequest = new ListObjectsRequest(Config.BucketName)
			{
				Prefix = dirName,
				Delimiter = "/"
			};
			var listResult = ossClient.ListObjects(listObjectsRequest);

			foreach (var prefix in listResult.CommonPrefixes)
			{
				keys.Add(prefix);
			}

			foreach (var summary in listResult.ObjectSummaries)
			{
				keys.Add(summary.Key);
			}

			if (!self)
			{
				keys.Remove(dirName);
			}

			return keys;
		}

		public List<string> GetFiles(string dirName, bool self = false)
		{
			dirName = GetDirName(dirName);
			var keys = new List<string>();
			var listObjectsRequest = new ListObjectsRequest(Config.BucketName)
			{
				Prefix = dirName,
			};
			var listResult = ossClient.ListObjects(listObjectsRequest);

			foreach (var prefix in listResult.CommonPrefixes)
			{
				keys.Add(prefix);
			}

			foreach (var summary in listResult.ObjectSummaries)
			{
				keys.Add(summary.Key);
			}

			if (!self)
			{
				keys.Remove(dirName);
			}

			return keys;
		}

		public void AppendFile(string fileName, Stream stream)
		{
			fileName = GetFileName(fileName);
			//第一次追加文件的时候，文件可能已经存在，先获取文件的当前长度，如果不存在，position为0
			long position = 0;

			if (ExistFile(fileName))
			{
				var metadata = ossClient.GetObjectMetadata(Config.BucketName, fileName);

				if (metadata.ObjectType == "Appendable")
				{
					position = metadata.ContentLength;
				}
				else
				{
					throw new HimallIOException(IOErrorMsg.NoramlFileNotOperate.ToDescription());
				}
			}
			else
			{
				RecurseCreateFileDir(fileName);
			}

			var request = new AppendObjectRequest(Config.BucketName, fileName)
			{
				ObjectMetadata = new ObjectMetadata(),
				Content = stream,
				Position = position
			};

			ossClient.AppendObject(request);
		}

		public void AppendFile(string fileName, string content)
		{
			fileName = GetFileName(fileName);
			//第一次追加文件的时候，文件可能已经存在，先获取文件的当前长度，如果不存在，position为0
			long position = 0;

			if (ExistFile(fileName))
			{
				var metadata = ossClient.GetObjectMetadata(Config.BucketName, fileName);
				if (metadata.ObjectType == "Appendable") //可追加文件
				{
					position = metadata.ContentLength;
				}
				else
				{
					throw new HimallIOException(IOErrorMsg.NoramlFileNotOperate.ToDescription());
				}
			}
			else
			{
				RecurseCreateFileDir(fileName);
			}

			byte[] binaryData = Encoding.UTF8.GetBytes(content);
			using (var stream = new MemoryStream(binaryData))
			{
				var request = new AppendObjectRequest(Config.BucketName, fileName)
				{
					ObjectMetadata = new ObjectMetadata(),
					Content = stream,
					Position = position
				};

				ossClient.AppendObject(request);
			}
		}

		public MetaInfo GetDirMetaInfo(string dirName)
		{
			dirName = GetDirName(dirName);
			if (ExistDir(dirName))
			{
				var metadata = ossClient.GetObjectMetadata(Config.BucketName, dirName);

				var metaInfo = new MetaInfo()
				{
					LastModifiedTime = metadata.LastModified.AddHours(8),
					ContentLength = metadata.ContentLength,
					ContentType = metadata.ContentType,
					ObjectType = metadata.ObjectType
				};


				return metaInfo;
			}
			else
			{
				throw new HimallIOException(IOErrorMsg.DirNotExist.ToDescription());
			}

		}

		public MetaInfo GetFileMetaInfo(string fileName)
		{
			fileName = GetFileName(fileName);
			if (ExistFile(fileName))
			{
				var metadata = ossClient.GetObjectMetadata(Config.BucketName, fileName);

				var metaInfo = new MetaInfo()
				{
					LastModifiedTime = metadata.LastModified.AddHours(8),
					ContentLength = metadata.ContentLength,
					ContentType = metadata.ContentType,
					ObjectType = metadata.ObjectType
				};

				return metaInfo;
			}
			else
			{
				throw new HimallIOException(IOErrorMsg.FileNotExist.ToDescription());
			}
		}

		/// <summary>
		/// 分级创建文件目录
		/// </summary>
		private void RecurseCreateFileDir(string fileName)
		{
			if (fileName.Contains("/"))
			{
				var dirs = fileName.Split('/').ToList();
				dirs.RemoveAt(dirs.Count - 1);

				RecurseCreateDir(dirs);
			}
		}

		/// <summary>
		/// 分级创建目录
		/// </summary>
		private void RecurseCreateDir(List<string> dirs)
		{
			string item = string.Empty;
			using (var stream = new MemoryStream())
			{
				foreach (var dir in dirs)
				{
					item += GetDirName(dir);
					if (!ExistDir(item))
					{
						ossClient.PutObject(Config.BucketName, item, stream);
					}
				}
			}
		}

		private string GetFileName(string fileName)
		{
			if (!string.IsNullOrWhiteSpace(fileName))
			{
				if (fileName.StartsWith("http://") || fileName.StartsWith("https://"))
					fileName = fileName.Substring(fileName.IndexOf("/", fileName.IndexOf("//") + 2) + 1);
				
				var index=fileName.LastIndexOf("@");
				if (index > 0)
					fileName = fileName.Substring(0, index);

				fileName = fileName.TrimStart('/');
			}
			return fileName;
		}

		private string GetDirName(string dirName)
		{
			if (string.IsNullOrWhiteSpace(dirName))
				return string.Empty;

			if (dirName.StartsWith("/"))
			{
				dirName = dirName.Substring(1);
			}
			if (!dirName.EndsWith("/"))
			{
				dirName = dirName + "/";
			}
			return dirName;
		}



		public void CreateThumbnail(string sourceFilename, string destFilename, int width, int height)
		{
			//OSS不需要处理缩略图
		}
	}
}
