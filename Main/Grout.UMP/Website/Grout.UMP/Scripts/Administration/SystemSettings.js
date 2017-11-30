var isKeyUp = false;
$(document).ready(function () {
    addPlacehoder("body");
    var loginFileExtension;
    var mainFileExtension;
    var favExtension;
    var loginFileName;
    var mainFileName;
    var favName;
    var currentDate = $.now();
    $("#mail_password").show();
    $("#upload-login-image").ejUploadbox({
        saveUrl: "/fileupload/Upload?imageType=loginlogo&&timeStamp=" + currentDate,
        autoUpload: true,
        showFileDetails: false,
        buttonText: { browse: ". . ." },
        extensionsAllow: ".PNG,.png,.jpg,.JPG,.jpeg,.JPEG",
        begin: function (e) {
            ShowWaitingProgress($("#upload-login-image").parents(".sub-section"), "show");
        },
        fileSelect: function (e) {
            loginFileExtension = e[0].extension.toLowerCase();
            loginFileName = e[0].name;

        },
        error: function (e) {
            if (loginFileExtension != ".png" && loginFileExtension != ".jpg" && loginFileExtension != ".jpeg") {
                $("#upload-login-image-textbox").addClass("ValidationErrorImage").val("Invalid file format");
                $("#upload-login-image-textbox").closest('div').addClass("has-error");
                $("#upload-login-image-textbox").parent().find('.e-box').addClass("upload-error-border");
            }
        },
        complete: function selectedFile(e) {
            SystemSettingsProperties.LoginLogo = "login_logo_" + currentDate + ".png";
            var imageURL = '/Content/Images/Application/' + "login_logo_" + currentDate + ".png?v=" + $.now();
            $("#login_logo_img").attr("src", imageURL);
            $("#upload-login-image-textbox").removeClass("ValidationErrorImage").val(loginFileName);
            $("#upload-login-image-textbox").closest('div').removeClass("has-error");
            $("#upload-login-image-textbox").parent().find('.e-box').removeClass("upload-error-border");
            ShowWaitingProgress($("#upload-login-image").parents(".sub-section"), "hide");

        }
    });

    $("#upload-Main-screen-image").ejUploadbox({
        saveUrl: "/fileupload/Upload?imageType=mainlogo&&timeStamp=" + currentDate,
        autoUpload: true,
        showFileDetails: false,
        buttonText: { browse: ". . ." },
        extensionsAllow: ".PNG,.png,.jpg,.JPG,.jpeg,.JPEG",
        begin: function () {
            ShowWaitingProgress($("#upload-Main-screen-image").parents(".sub-section"), "show");
        },
        fileSelect: function (e) {
            mainFileExtension = e[0].extension.toLowerCase();
            mainFileName = e[0].name;

        },
        error: function (e) {
            if (mainFileExtension != ".png" && mainFileExtension != ".jpg" && mainFileExtension != ".jpeg") {
                $("#upload-Main-screen-image-textbox").addClass("ValidationErrorImage").val("Invalid file format");
                $("#upload-Main-screen-image-textbox").closest('div').addClass("has-error");
                $("#upload-Main-screen-image-textbox").parent().find('.e-box').addClass("upload-error-border");
            }
        },
        complete: function selectedFile(e) {
            SystemSettingsProperties.MainScreenLogo = "main_logo_" + currentDate + ".png";
            var imageURL = '/Content/Images/Application/' + "main_logo_" + currentDate + ".png?v=" + $.now();
            $("#mainscreen_logo_img").attr("src", imageURL);
            $("#upload-Main-screen-image-textbox").removeClass("ValidationErrorImage").val(mainFileName);
            $("#upload-Main-screen-image-textbox").closest('div').removeClass("has-error");
            $("#upload-Main-screen-image-textbox").parent().find('.e-box').removeClass("upload-error-border");
            ShowWaitingProgress($("#upload-Main-screen-image").parents(".sub-section"), "hide");
        }
    });

    $("#upload-favicon-image").ejUploadbox({
        saveUrl: "/fileupload/Upload?imageType=favicon&&timeStamp=" + currentDate,
        autoUpload: true,
        showFileDetails: false,
        buttonText: { browse: ". . ." },
        extensionsAllow: ".PNG,.png,.jpg,.JPG,.jpeg,.JPEG",
        begin: function () {
            ShowWaitingProgress($("#upload-favicon-image").parents(".sub-section"), "show");
        },
        fileSelect: function (e) {
            favExtension = e[0].extension.toLowerCase();
            favName = e[0].name;

        },
        error: function (e) {
            if (favExtension != ".png" && favExtension != ".jpg" && favExtension != ".jpeg") {
                $("#upload-favicon-image-textbox").addClass("ValidationErrorImage").val("Invalid file format");
                $("#upload-favicon-image-textbox").closest('div').addClass("has-error");
                $("#upload-favicon-image-textbox").parent().find('.e-box').addClass("upload-error-border");
            }
        },
        complete: function selectedFile(e) {
            SystemSettingsProperties.FavIcon = "favicon_" + currentDate + ".png";
            var imageURL = '/Content/Images/Application/' + "favicon_" + currentDate + ".png?v=" + $.now();
            $(".favicon-image").find('img').attr("src", imageURL);
            $("#upload-favicon-image-textbox").removeClass("ValidationErrorImage").val(favName);
            $("#upload-favicon-image-textbox").closest('div').removeClass("has-error");
            $("#upload-favicon-image-textbox").parent().find('.e-box').removeClass("upload-error-border");
            ShowWaitingProgress($("#upload-favicon-image").parents(".sub-section"), "hide");
        }
    });

    $("div.date-format-radio input[type=radio]").each(function () {
        if (this.value == SystemSettingsProperties.DateFormat) {
            $("#" + this.id).attr('checked', 'checked');
        }
    });

    $(".mail-settings-fields:not('#mail_password')").keyup(function (e) {
        if ($("#mail_password").val() == "") {
            if (parseInt($("#port_number").val()) != SystemSettingsProperties.MailSettingsPort ||
                $("#smtp_address").val() != SystemSettingsProperties.MailSettingsHost ||
                $("#mail_display_name").val() != SystemSettingsProperties.MailSettingsSenderName ||
                $("#mail_user_name").val() != SystemSettingsProperties.MailSettingsAddress ||
                $("#mail_secure_auth").data("ejCheckBox").model.checked != SystemSettingsProperties.MailSettingsIsSecureAuthentication) {
                $("#mail_password").attr("placeholder", "Please enter password");
                $("#mail_password").siblings(".placeholder").html("Please enter password");
            } else {
                $("#mail_password").attr("placeholder", "••••••••");
                $("#mail_password").siblings(".placeholder").html("••••••••");
            }
        }
    });

    $(".show-hide-password").on('mousedown', function () {
        if ($(this).siblings("input").is(":password")) {
            $(this).siblings("input").attr('type', 'text');
        } else {
            $(this).siblings("input").attr('type', 'password');
        }
    });
    $(".show-hide-password").on('mouseup', function () {
        if ($(this).siblings("input").is(":password")) {
            $(this).siblings("input").attr('type', 'text');
        } else {
            $(this).siblings("input").attr('type', 'password');
        }
    });

    $("#MM_DD_YYYY").ejRadioButton({ size: "medium", checked: ($("#MM_DD_YYYY").val() != SystemSettingsProperties.DateFormat) ? false : true });
    $("#DD_MM_YYYY").ejRadioButton({ size: "medium", checked: ($("#DD_MM_YYYY").val() != SystemSettingsProperties.DateFormat) ? false : true });
    $("#DD_MMM_YYYY").ejRadioButton({ size: "medium", checked: ($("#DD_MMM_YYYY").val() != SystemSettingsProperties.DateFormat) ? false : true });
    $("#MMM_DD_YYYY").ejRadioButton({ size: "medium", checked: ($("#MMM_DD_YYYY").val() != SystemSettingsProperties.DateFormat) ? false : true });



    $("#UpdateSystemSettings,#UpdateSystemSettings-bottom").on('click', function () {
        $(".confirmatioMessage").hide();

        if (!$("#look-and-feel-container").valid() || !$('#email-setting-body').valid()) {
            return;
        }
        var isUrlChange = false;
        if ($("#site_url").attr("data-original-value") != $("#site_url").val()) {
            isUrlChange = true;
        }
        var isMailSettingsChanged = false;
        var isMailPasswordChanged = false;
        var mailSettings = new Object;
        if (parseInt($("#port_number").val()) != SystemSettingsProperties.MailSettingsPort
            || $("#smtp_address").val() != SystemSettingsProperties.MailSettingsHost
            || $("#mail_display_name").val() != SystemSettingsProperties.MailSettingsSenderName
            || $("#mail_user_name").val() != SystemSettingsProperties.MailSettingsAddress
            || $("#mail_secure_auth").data("ejCheckBox").model.checked != SystemSettingsProperties.MailSettingsIsSecureAuthentication) {

            isMailSettingsChanged = true;

            mailSettings = {
                Address: $("#mail_user_name").val(),
                Password: $("#mail_password").val(),
                Host: $("#smtp_address").val(),
                SenderName: $("#mail_display_name").val(),
                Port: parseInt($("#port_number").val()),
                IsSecureAuthentication: $("#mail_secure_auth").data("ejCheckBox").model.checked
            }
        }

        if ($("#mail_password").val() != "") {
            isMailPasswordChanged = true;
        }

        var systemSettingsData = {
            OrganizationName: $("#site-orgname").val(),
            LoginLogo: SystemSettingsProperties.LoginLogo,
            MainScreenLogo: SystemSettingsProperties.MainScreenLogo,
            FavIcon: SystemSettingsProperties.FavIcon,
            WelcomeNoteText: $("#txt_welcome_note").val(),
            TimeZone: $("#time_zone").val(),
            DateFormat: $('input:radio[name=date_format]:checked').val(),
            MailSettingsAddress: $("#mail_user_name").val(),
            MailSettingsPassword: $("#mail_password").val(),
            MailSettingsHost: $("#smtp_address").val(),
            MailSettingsSenderName: $("#mail_display_name").val(),
            MailSettingsPort: parseInt($("#port_number").val()),
            MailSettingsIsSecureAuthentication: $("#mail_secure_auth").data("ejCheckBox").model.checked,
            BaseUrl: $("#site_url").val(),
            ActiveDirectoryUserName: $("#username").val(),
            ActiveDirectoryPassword: $("#password").val(),
            LdapUrl: $("#ldapurl").val()
        };

        $.ajax({
            type: "POST",
            url: "/Administration/UpdateSystemSettings",
            data: { systemSettingsData: JSON.stringify(systemSettingsData) },
            beforeSend: showWaitingPopup($("#base_container")),
            success: function (data) {
                if (isUrlChange) {
                    window.location.href = $("#site_url").val() + "/" + location.pathname;
                } else {
                    $("#main_screen_logo a img").attr("src", "/Content/Images/Application/" + systemSettingsData.MainScreenLogo);
                    var link = document.createElement('link');
                    link.type = 'image/x-icon';
                    link.rel = 'shortcut icon';
                    link.href = "/Content/Images/Application/" + systemSettingsData.FavIcon;
                    document.getElementsByTagName('head')[0].appendChild(link);
                    var pageTitle = $("#site-orgname").val() + " - " + document.title.split("-")[1];
                    document.title = pageTitle;
                }
                hideWaitingPopup($("#base_container"));
                $(".confirmatioMessage").show();
                $(".error-message, .success-message").css("display", "none");
            }
        });
    });

    $.validator.addMethod("isValidUrl", function (value, element) {
        if (isValidUrl(value) == false && value.indexOf("localhost") == -1)
            return false;
        else
            return true;

    }, "Please enter valid URL");

    $.validator.addMethod("isRequired", function (value, element) {
        return !isEmptyOrWhitespace(value);
    }, "Please enter the name");

    $.validator.addMethod("isValidEmail", function (value, element) {
        if (isKeyUp)
            return true;
        else
            return IsEmail(value)
    }, "Invalid email");


    $("#look-and-feel-container").validate({
        errorElement: 'span',
        onkeyup: function (element, event) { if (event.keyCode != 9) $(element).valid(); else true; },
        onfocusout: function (element) { $(element).valid(); },
        rules: {
            "site_url": {
                isRequired: true,
                isValidUrl: true

            }
        },
        highlight: function (element) {
            $(element).closest('div').addClass("has-error");
        },
        unhighlight: function (element) {
            $(element).closest('div').removeClass('has-error');
            $(element).parent().find("span.validation-errors").html("");

        },
        errorPlacement: function (error, element) {
            $(element).parent().find("span.validation-errors").html(error);
        },
        messages: {
            "site_url": {
                isRequired: "Please enter URL"
            }
        }
    });

    $('#email-setting-body').validate({
        errorElement: 'span',
        onkeyup: function (element, event) {
            if (event.keyCode != 9) {
                isKeyUp = true;
                $(element).valid();
                isKeyUp = false;
            }
            else
                true;
        },
        onfocusout: function (element) { $(element).valid(); },
        rules: {
            "smtp_address": {
                isRequired: true
            },
            "port_number": {
                isRequired: true
            },
            "mail_display_name": {
                isRequired: true
            },
            "mail_user_name": {
                isRequired: true,
                isValidEmail: true
            },
            "mail_password": {
                required: true
            }
        },
        highlight: function (element) {
            $(element).closest('.form-input-field').addClass("has-error");
        },
        unhighlight: function (element) {
            $(element).closest('.form-input-field').removeClass('has-error');
            $(element).parent().find("span.validation-errors").html("");

        },
        errorPlacement: function (error, element) {
            $(element).parent().find("span.validation-errors").html(error);
        },
        messages: {
            "smtp_address": {
                isRequired: "Please enter SMTP server"
            },
            "port_number": {
                isRequired: "Please enter SMTP port"
            },
            "mail_display_name": {
                isRequired: "Please enter sender name"
            },
            "mail_user_name": {
                isRequired: "Please enter sender email"
            },
            "mail_password": {
                required: "Please enter password"

            }
        }

    });
});