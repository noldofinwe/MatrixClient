using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Shared.Image;
using Storage.Contexts;

namespace Storage.Models.Account
{
    public class ImageModel: AbstractModel
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        [NotMapped]
        private int _id;

        /// <summary>
        /// The actual image data.
        /// </summary>
        [Required]
        public byte[] data
        {
            get => _data;
            set => SetDataProperty(value);
        }
        [NotMapped]
        private byte[] _data;

        /// <summary>
        /// SHA-1 hash of the <see cref="data"/> in hex.
        /// </summary>
        [Required]
        public string hash
        {
            get => _hash;
            set => SetProperty(ref _hash, value);
        }
        [NotMapped]
        private string _hash;

        /// <summary>
        /// The IANA media type of the image.
        /// https://www.iana.org/assignments/media-types/media-types.xhtml#image
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Basics_of_HTTP/MIME_types
        /// </summary>
        [Required]
        public string type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }
        [NotMapped]
        private string _type;

        [NotMapped]
        private Bitmap img;
        [NotMapped]
        private IImage imgSrc;

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--


        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--
        public async Task<Bitmap> GetSoftwareBitmapAsync()
        {
            if (img is null && data is not null && data.Length > 0)
            {
                using var stream = new MemoryStream(data);
                img = await Task.Run(() => Bitmap.DecodeToWidth(stream, 256)); // Optional width
            }

            return img;
        }

        public async Task<IImage> GetImageSourceAsync()
        {
            if (imgSrc is null)
            {
                if (img is null)
                {
                    img = await GetSoftwareBitmapAsync(); // Your method to load the Bitmap
                }

                imgSrc = img; // Bitmap implements IImage in Avalonia
            }

            return imgSrc;
        }

        private void SetDataProperty(byte[] value)
        {
            if (SetProperty(ref _data, value))
            {
                img = null;
                imgSrc = null;
            }
        }

        public async Task SetImageAsync(Bitmap img, bool isAnimated)
        {
            Debug.Assert(img is not null);

            using var stream = new MemoryStream();
            img.Save(stream); // Saves as PNG by default
            data = stream.ToArray();

            hash = ImageUtils.HashImage(data); // Your custom hash method

            this.img = img;
            imgSrc = img; // Bitmap implements IImage in Avalonia
        }


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--
        public override void Remove(MainDbContext ctx, bool recursive)
        {
            ctx.Remove(this);
        }

        public override bool Equals(object obj)
        {
            return obj is ImageModel img && string.Equals(hash, img.hash);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        #region --Misc Methods (Private)--


        #endregion

        #region --Misc Methods (Protected)--


        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}
