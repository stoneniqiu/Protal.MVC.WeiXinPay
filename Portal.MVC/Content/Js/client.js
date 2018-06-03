var Client = function() {
    var self = this;
    self.loadImg = function(buttonId, callback) {
        var uploader = new plupload.Uploader({
            runtimes: 'html5,flash,silverlight,html4',
            browse_button: buttonId,
            url: '/Home/UploadImg',
            flash_swf_url: '/Content/plupload-2.1.8/js/Moxie.swf',
            silverlight_xap_url: '/Content/plupload-2.1.8/js/Moxie.xap',
            filters: {
                max_file_size: "3mb",
                mime_types: [
                    { title: "Image files", extensions: "jpg,gif,png,bmp" },
                    //{ title: "Zip files", extensions: "zip" }
                ]
            },
            init: {
                PostInit: function() {
                },
                FilesAdded: function(up, files) {
                    plupload.each(files, function(file) {
                        uploader.start();
                    });
                },
                UploadProgress: function(up, file) {
                },
                Error: function(up, err) {
                    if (err.message) {
                        alert(err.message);
                    }
                }
            }
        });
        uploader.init();
        uploader.bind('FileUploaded', function(upldr, file, object) {
            var data = JSON.parse(object.response);
            console.log(data);
            callback(data);
        });
    };
    self.loadFile = function (buttonId, callback) {
        var uploader = new plupload.Uploader({
            runtimes: 'html5,flash,silverlight,html4',
            browse_button: buttonId,
            url: '/Home/UploadFile',
            flash_swf_url: '/Content/plupload-2.1.8/js/Moxie.swf',
            silverlight_xap_url: '/Content/plupload-2.1.8/js/Moxie.xap',
            filters: {
                max_file_size: "3mb",
                mime_types: [
                    { title: "files", extensions: "xls,xlsx" },
                    //{ title: "Zip files", extensions: "zip" }
                ]
            },
            init: {
                PostInit: function () {
                },
                FilesAdded: function (up, files) {
                    plupload.each(files, function (file) {
                        uploader.start();
                    });
                },
                UploadProgress: function (up, file) {
                },
                Error: function (up, err) {
                    if (err.message) {
                        alert(err.message);
                    }
                }
            }
        });
        uploader.init();
        uploader.bind('FileUploaded', function (upldr, file, object) {
            var data = JSON.parse(object.response);
            console.log(data);
            callback(data);
        });
    };
};
window.client = new Client();
