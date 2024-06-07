using Microsoft.AspNetCore.Mvc;
using QRCODE.Context;
using QRCODE.Models;
using System.Drawing;
using System.IO;
using ZXing;


namespace QRCODE.Controllers
{
    public class CameraController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly MyContext _myContext;

        public CameraController(IWebHostEnvironment environment, MyContext myContext)
        {
            _environment = environment;
            _myContext = myContext;
        }

        public IActionResult Capture()
        {
            return View();
        }

        [HttpPost] 
        public IActionResult Capture(string name)
        {
            try
            {
                var files = HttpContext.Request.Form.Files;
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            var stringQR = String.Empty;
                            var fileName = file.FileName;
                            System.Guid guid = System.Guid.NewGuid();
                            string newguid = guid.ToString();
                            var fileNameUnik = String.Concat(DateTime.Today.ToString("ddMMyyyy"),"_", Convert.ToString(newguid));
                            var fileExt = Path.GetExtension(fileName);
                            var newFileName = String.Concat(fileNameUnik, fileExt);
                            var filePath = Path.Combine(_environment.WebRootPath, "QrCaptured")+$@"\{newFileName}";

                            if (!string.IsNullOrEmpty(filePath))
                            {
                                StoreInLocals(file, filePath);
                            }

                            var imgBytes = System.IO.File.ReadAllBytes(filePath);
                            if (imgBytes != null && stringQR != null)
                            {
                                StoreInDB(imgBytes, stringQR, newFileName);
                            }

                            var readQR = new BarcodeReaderGeneric();
                            Bitmap imgs = Image.FromFile(filePath) as Bitmap;
                            
                            try
                            {
                                using (imgs)
                                {
                                    LuminanceSource source;
                                    source = new ZXing.Windows.Compatibility.BitmapLuminanceSource(imgs);
                                    Result hasil = readQR.Decode(source);

                                    if (hasil != null)
                                    {
                                        stringQR = hasil.ToString();                                        
                                        //update status & lastupdate
                                        var updateRecord = _myContext.qrstores.First(q => q.QRBASE64 == newFileName);
                                        var selectId = _myContext.qrstores.First(s => s.QRID == updateRecord.QRID);

                                        if (updateRecord != null)
                                        {
                                            selectId.QRSTRING = stringQR;
                                            selectId.LASTUPDATE = DateTime.Now;
                                            selectId.STATUS = "Generate SrtingQR - Succeed";

                                            _myContext.Entry(selectId).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                                            var update = _myContext.SaveChanges();
                                        }
                                        //ViewBag.Text = String.Concat("QR Code Valid, Content of QR String is: ", stringQR);
                                        //return View("Capture");
                                    }
                                    else
                                    {
                                        var updateRecord = _myContext.qrstores.First(q => q.QRBASE64 == newFileName);
                                        var selectId = _myContext.qrstores.First(s => s.QRID == updateRecord.QRID);

                                        if (updateRecord != null)
                                        {
                                            selectId.QRSTRING = String.Empty;
                                            selectId.LASTUPDATE = DateTime.Now;
                                            selectId.STATUS = "QRCode Invalid";

                                            _myContext.Entry(selectId).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                                            var update = _myContext.SaveChanges();
                                        }
                                        ViewBag.Text = String.Format("QR Code Invalid, QRScan Failed");
                                    }
                                }                                
                            }
                            catch
                            {
                                throw new Exception("Invalid QRCode - Cannot decode QRCode");
                            }
                        }
                    }
                    return Json(true);
                }
                else
                {
                    return Json(false);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private void StoreInDB(byte[] imgBytes, String strQR, String newFileName)
        {
            try
            {
                if (imgBytes != null && strQR != null)
                {
                    String imgBase64 = Convert.ToBase64String(imgBytes, 0, imgBytes.Length);
                    String imgUrl = String.Concat("data:image/jpg;base64,", imgBase64);
                    qrstored imgStore = new qrstored()
                    {
                        CREATEDDATE = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                        QRBASE64 = newFileName,
                        QRSTRING = strQR,
                        ISSTORED = "1",
                        STATUS = "Processing Generate String QR",
                        LASTUPDATE = DateTime.Now
                        //QRID = 1
                    };
                    _myContext.qrstores.Add(imgStore);
                    _myContext.SaveChanges();


                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void StoreInLocals(IFormFile file, string fileName)
        {
            using (FileStream fileStream = System.IO.File.Create(fileName))
            {
                file.CopyTo(fileStream);
                fileStream.Flush();
            }
        }
    }
}
