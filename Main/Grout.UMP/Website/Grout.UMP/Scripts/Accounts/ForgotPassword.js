var userAgent = navigator.userAgent;
var regexIe8 = new RegExp("Trident(\/4.0)|(Trident\/5.0)");

$(function () {
    addPlacehoder("body");
    $("#Confirm_password,#new_password").keyup(function (event) {
        if (event.keyCode == 13) {
            $("#Save_Activate").click();
        }
    });
    $("#Save_Activate").on('click', function () {             
        if ($("#Account_Activation_Fields_Container").valid()) {
            $.ajax({
                type: "POST",
                url: "/accounts/SavePasswordActivate",
                data: { UserId: userid, Password: $("#new_password").val() },
                success: function (data, result) {
                    if (data != 'Failure') {
                        $('#Account_Activation_Fields_Container').remove();
                        $("#Account_Activated_Message_Container").css("display", "block");
                    } else {
                        $("#password_validate").html("");
                        $("#re_password_validate").html("* Password should contain 5 characters!");
                    }
                }
            });
        }
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

    $.validator.addMethod("isRequired", function (value, element) {
        if ($.trim(value) != '')
            return true;
        else
            return false;
    }, "Please enter the name");

    $('#forgot-password-form').validate({       
        onkeyup: function (element) { $(element).valid(); },
        onfocusout: function (element) { $(element).valid(); },
        rules: {
            key: {
                isRequired: true
            }          
        },
        highlight: function (element,error) {
            $(element).closest('.forgotinput').addClass("has-error");            
        },
        unhighlight: function (element) {
            $(element).closest('.forgotinput').removeClass('has-error');
            $(element).closest("form").find("#Usernamevalidation").html("");
        },
        errorElement:'span',
        errorPlacement: function (error, element) {
            $(element).closest("form").find("#Usernamevalidation").html(error);
        },
        messages: {           
            key: {
                isRequired: "Please enter your username or email address"
            }
        }
    });

    $("#Account_Activation_Fields_Container").validate({
        onkeyup: function (element, event) { if (event.keyCode != 9) $(element).valid(); else true; },
        onfocusout: function (element) { $(element).valid(); },
        rules: {
            "new_password": {
                required: true,
                minlength:5
            },
            "Confirm_password": {
                required: true,
                equalTo: "#new_password"
            }
        },
        messages: {
            "new_password": {
                required: "* Please enter password",
                minlength: "* Password should contain 5 characters!"
            },
            "Confirm_password": {
                required: "* Please confirm password",
                equalTo: "* Passwords mismatch"
            }
        },
        highlight: function (element) {
            $(element).closest('div').addClass("has-error");
        },
        unhighlight: function (element) {
            $(element).closest('div').removeClass('has-error');
            $(element).closest('div').find(">.validation-messages").html("");

        },
        errorElement: 'span',
        errorPlacement: function (error, element) {
            $(element).closest('div').find(">.validation-messages").html(error);
        }

    });


    $("#update-password-form").validate({
        onkeyup: function (element, event) { if (event.keyCode != 9) $(element).valid(); else true; },
        onfocusout: function (element) { $(element).valid(); },
        rules: {
            "password": {
                required: true,
                minlength: 5
            },
            "re-password": {
                equalTo:"#password"               
            }
        },
        messages: {
            "password": {
                required: "* Please enter password",
                minlength: "* Password should contain 5 characters!"
            },
            "re-password": {
                equalTo: "* Password mismatch"
            }
        },
        highlight: function (element) {
            $(element).closest('div').addClass("has-error");
        },
        unhighlight: function (element) {
            $(element).closest('div').removeClass('has-error');
            $(element).closest('form').find("#validation_error").html("");

        },
        errorElement: 'span',
        errorPlacement: function (error, element) {
            $(element).closest('form').find("#validation_error").html(error);
        }
    });


});

function changePasswordValidation() {  
    return $("#update-password-form").valid();
}

function ForgotValidate() {
    return $('#forgot-password-form').valid();
}


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