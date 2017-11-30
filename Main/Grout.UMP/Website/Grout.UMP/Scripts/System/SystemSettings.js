var userAgent = navigator.userAgent;
var regexIe8 = new RegExp("Trident(\/4.0)|(Trident\/5.0)");
var isKeyUp=false;
$(document).ready(function () {

    $(".dbselect").ejRadioButton({ size: "medium", checked: false });

    $(window).resize(function () {
        changeFooterPostion();
    });

    changeFooterPostion();
    var height = $(document).height();
    $("#startupPageConatiner").css("height", height);

    $("input[name='DatabaseType']").on("click change", function () {
        var checkedVal = $("input[name='DatabaseType']:checked").val();
        if (checkedVal == 'MSSQL') {
            $("#db_content_holder").show();
            $("#MovetoNext").hide();
            $("#db_config_submit").show();
        } else {
            $(".txt-holder input[type='text'], .txt-holder input[type='password']").val('');
            $(".validation-txt-errors").hide();
            $(".validation-errors").html("");
            $(".has-error").removeClass("has-error");
            $("#db_content_holder").hide();
            $("#db_config_submit").hide();
            $("#MovetoNext").show();
        }
        addPlacehoder("#system-settings-container-page1");
        changeFooterPostion();
    });

    $("#Checkwindows").on("click change", function () {
        var windowsCheck = $("#Checkwindows").val() == "windows";
        var databaseType = $("input[name='DatabaseType']:checked").val();
        if (windowsCheck && databaseType == 'MSSQL') {
            $("#txt_login").val("").attr("disabled", true);
            $("#txt_password_db").val("").attr("disabled", true);
        }
        else if (databaseType == 'MSSQL') {
            $("#txt_login").attr("disabled", false);
            $("#txt_password_db").attr("disabled", false);
        }
        $(".has-error").removeClass("has-error");
        $(".validation-txt-errors").hide();
    });

    $("#db_config_submit").on("click", function () {
        $(".has-error").removeClass("has-error");
        $(".validation-txt-errors").hide();
        $(".validation-errors").html("");
        var canProceed = $('#db_content_holder').valid();      
        if (canProceed) {
            showWaitingPopup($(".startupPageConatiner"));
            $(this).prop("disabled", true);
            $("#db_loader").show();
            window.serverName = $("#txt_servername").val();
            window.IsWindowsAuthentication = $("#Checkwindows").val() == "windows";
            window.login = $("#txt_login").val();
            window.password = $("#txt_password_db").val();
            window.databaseName = $("#txt_dbname").val();
            doAjaxPost("POST", "../SystemStartUpPage/ConnectDatabase",
                {
                    data: JSON.stringify({ serverName: window.serverName, userName: window.login, password: window.password, IsWindowsAuthentication: window.IsWindowsAuthentication, databaseName: window.databaseName })
                },
                function (result) {


                    if (result.Data.key) {
                        var databaseType = $("input[name='DatabaseType']:checked").val();
                        doAjaxPost("POST", "../SystemStartUpPage/GenerateDatabase",
                            {
                                data: JSON.stringify({ ServerType: databaseType, serverName: window.serverName, userName: window.login, password: window.password, IsWindowsAuthentication: window.IsWindowsAuthentication, databaseName: window.databaseName })
                            },
                            function (result) {
                                hideWaitingPopup($(".startupPageConatiner"));
                                if (result.Data.key) {
                                    $(".selected").removeClass("selected");
                                    $("ul li[data-move-to='startup-page-two']").addClass("selected");
                                    $("#db_loader").hide();
                                    $("#system-settings-container-page1").hide();
                                    $("#system-settings-container-page2").show();
                                    $("#txt_username").focus();
                                    window.connectionString = result.Data.value;
                                    delete window.serverName;
                                    delete window.login;
                                    delete window.password;
                                    delete window.databaseName;
                                }
                                else {
                                    $("#db_config_submit").prop("disabled", false);
                                    $("#db_loader").hide();
                                    $("#connection_validation").find(".validation-errors").html(result.Data.value);
                                }
                                changeFooterPostion();
                                $("#startupPageConatiner").css("height", $(document).height());
                            }
                        );
                        $(".db-connect-outer-container").find(".title").html("Database Creation!");
                        $("#txt_dbname").focus();
                    }
                    else {
                        hideWaitingPopup($(".startupPageConatiner"));
                        $("#db_config_generate").hide();
                        $("#db_config_submit").show();
                        $("#db_config_submit").prop("disabled", false);
                        $("#db_loader").hide();
                        $("#connection_validation").find(".validation-errors").html(result.Data.value);
                    }
                }
            );
        }
    });
	$("#db_content_holder").on("keyup", "input", function (event) {
		if (event.keyCode == 13) {
			$("#db_config_submit").click();
		}
	});
    $(document).on('click', '#MovetoNext', function () {
        showWaitingPopup($(".startupPageConatiner"));
        var databaseType = $("input[name='DatabaseType']:checked").val();
        $("#db_loader").show();
        doAjaxPost("POST", "../SystemStartUpPage/GenerateDatabase",
            {
                data: JSON.stringify({ ServerType: databaseType, serverName: window.serverName, userName: window.login, password: window.password, databaseName: window.databaseName })
            },
            function (result) {
                $("#startupPageConatiner").css("height", "");
                hideWaitingPopup(".startupPageConatiner");
                if (result.Data.key) {
                    $(".selected").removeClass("selected");
                    $("ul li[data-move-to='startup-page-two']").addClass("selected");
                    $("#db_loader").hide();
                    $("#system-settings-container-page1").hide();
                    $("#system-settings-container-page2").show();
                    window.connectionString = result.Data.value;
                    changeFooterPostion();
                    delete window.serverName;
                    delete window.login;
                    delete window.password;
                    delete window.databaseName;
                    addPlacehoder("body")
                }
                else {
                    $("#db_config_generate").prop("disabled", false);
                    $("#db_loader").hide();
                    $("#connection_validation").find(".validation-errors").html(result.Data.value);
                }
            }
        );
    });

    $(".show-hide-password").on('mousedown', function () {
        if ($(this).siblings("input").is(":password")) {
            $(this).siblings("input").attr('type', 'text');
        }
        else {
            $(this).siblings("input").attr('type', 'password');
        }
    });

    $(".show-hide-password").on('mouseup', function () {
        if ($(this).siblings("input").is(":password")) {
            $(this).siblings("input").attr('type', 'text');
        }
        else {
            $(this).siblings("input").attr('type', 'password');
        }
    });

    $(".show-hide-password").mouseleave(function () {
        $(this).siblings("input").attr('type', 'password');
    });

    $("#btn_proceed_page1").on('click', function () {
        $("#steps-container").find('.selected').removeClass('selected');
        $("li[data-move-to='startup-page-two']").addClass('selected');

        $("#system-settings-container-page1").hide();
        $("#system-settings-container-page2").show();
        $("#txt_username").focus();
        changeFooterPostion();
    });


    $("#btn_proceed_page2").on('click', function () {

        if ($('.admin-account-fields-container').valid()) {
            showWaitingPopup($(".startupPageConatiner"));
            var serverType = $("input[name='DatabaseType']:checked").val();
            if (serverType == 'MSSQL')
                serverType = 0;
            else
                serverType = 2;

            var authenticationType = 1;
            if (!($("#Checkwindows").val() == "windows"))
                authenticationType = 0;

            var globalAdmin = {
                UserName: $("#txt_username").val(),
                FirstName: $("#txt_firstname").val(),
                LastName: $("#txt_lastname").val(),
                Email: $("#txt_emailid").val(),
                Password: $("#txt_password").val()
            };
            var systemSettingsData = {
                SQLConfiguration:
                    {
                        ConnectionString: window.connectionString,
                        ServerType: serverType,
                        AuthenticationType: authenticationType
                    }
            };

            var data = {
                globalAdminDetails: JSON.stringify(globalAdmin),
                systemSettingsData: JSON.stringify(systemSettingsData)
            };

            $.ajax({
                type: "POST",
                url: "/SystemStartUpPage/SetSystemSettings",
                data: data,
                success: function (data, result) {
                    window.location.href = "/login";
                },
                error: function (data, result) {
                    window.location.href = "/startup";
                }
            });
        }
    });

    $(".admin-account-fields-container").on("keyup", "input", function (event) {
        if (event.keyCode == 13) {
            $("#btn_proceed_page2").click();
        }
    });


    $.validator.addMethod("isRequired", function (value, element) {
        return !isEmptyOrWhitespace(value);
    }, "Please enter the name");

    $.validator.addMethod("hasWhiteSpace", function (value, element) {
        return /\s/.test(value);
    }, "Username contains space");

    $.validator.addMethod("isValidUser", function (value, element) {
        return isValidUserName(value)
    }, "Username contains invalid characters");

    $.validator.addMethod("isValidEmail", function (value, element) {
        return validateEmail(value)
    }, "Please enter a valid email address");

    $('.admin-account-fields-container').validate({    
        focusInvalid:false,
        errorElement: "span",
        onkeyup: function (element, event) {
            if (event.keyCode != 9) {
                isKeyUp = true;
                $(element).valid();
                isKeyUp = false;
            }
            else
                true;           
			},
        onfocusout: function (element) { 
			$(element).valid(); 
			},
        rules: {
            username: {
                isRequired: true,
                hasWhiteSpace: false,
                isValidUser:true              
            },
            firstname: {
                isRequired: true
            },
            email: {
                isRequired: true,
                isValidEmail:true
            },
            password: {
                required: true
            },
            confirm: {
                required: true,
                equalTo: "#txt_password"
            }
        },     
        highlight:function(element) {
            $(element).closest('.form-group').addClass("has-error");
            $(element).parent().find(">.su-login-error").show();
        },
        unhighlight: function (element) {
            $(element).closest('.form-group').removeClass('has-error');
            $(element).parent().find(">.su-login-error").hide();
        },
        errorPlacement: function (error, element) {
            $(element).parent().find(">.su-login-error").attr("title",error.html());
        },
        messages:{
            username: {
                isRequired: "Please enter username"
            },
            firstname: {
                isRequired: "Please enter first name"
            },
            email: {
                isRequired: "Please enter email address"
            },
            password: {
                required: "Please enter password"
            },
            confirm: {
                required: "Please confirm password",
                equalTo: "Passwords Mismatch"
            }
        }
        
    });

    $('#db_content_holder').validate({
        errorElement: "span",
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
            servername: {
                isRequired: true
            },
            username: {
                isRequired: true
            },
            password: {
                required: true                
            },
            dbname: {
                isRequired: true
            }            
        },
        highlight: function (element) {
            $(element).closest('.txt-holder').addClass("has-error");
            $(element).parent().find(">.su-login-error").show();
        },
        unhighlight: function (element) {
            $(element).closest('.txt-holder').removeClass('has-error');
            $(element).parent().find(">.su-login-error").hide();
        },
        errorPlacement: function (error, element) {
            $(element).parent().find(">.su-login-error").attr("title", error.html());
        },
        messages: {
            servername: {
                isRequired: "Please enter server name"
            },
            username: {
                isRequired: "Please enter username"
            },           
            password: {
                required: "Please enter password"
            },
            dbname: {
                isRequired: "Please enter the database name"
            }
        }
    });


});

function validateEmail(email,eventType) {
	if(isKeyUp){
		return true;
	}
	else{
		var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
		return re.test(email);
	}
}

function changeFooterPostion() {
    if (window.innerHeight - $("#system_settings_general").height() > 70) {
        $("#system_settings_footer").addClass("footer-fixed");
    } else {
        $("#system_settings_footer").removeClass("footer-fixed");
    }
}

function isValidUserName(userName) {
    var filter = /^[A-Za-z0-9][A-Za-z0-9]*([._-][A-Za-z0-9]+){0,3}$/;
    if (filter.test(userName)) {
        return true;
    } else {
        return false;
    }
}


$(document).on("focus", "input[type=text],input[type=password]", function () {
    if (regexIe8.test(userAgent)) {
        $(this).next(".placeholder").removeClass("show").addClass("hide");
    }
});

$(document).on("focusout", "input[type=text],input[type=password]", function () {
    if (regexIe8.test(userAgent) && $(this).val() === "") {
        $(this).next(".placeholder").removeClass("hide").addClass("show");
    }
});
$(document).on("focus", ".placeholder", function () {
    $(this).prev("input").focus();
});

function addPlacehoder(object) {
    if (regexIe8.test(userAgent)) {
        $(object).find("input[type=text],input[type=password]").each(function () {
            if ($(this).val() === "") {
                $($("<div>", { "class": "placeholder", text: $(this).attr("placeholder") })).insertAfter(this).show();
            }
            else {
                $($("<div>", { "class": "placeholder", text: $(this).attr("placeholder") })).insertAfter(this).hide();
            }
        });
    }

}
