$(document).ready(function () {
    addPlacehoder("body");
    $('input[name=username]').focus();

    $.validator.addMethod("isRequired", function (value, element) {
    return !isEmptyOrWhitespace(value);
    }, "Please enter the name");
    $('#login-form').validate({
        onkeyup: function (element, event) { if (event.keyCode != 9) $(element).valid(); else true; },
        onfocusout: function (element) { $(element).valid(); },
        rules: {
            username: {
                isRequired: true
            },
            password: {
                required: true
            }
        },
        highlight: function (element) {
            $(element).closest('.login-fields').addClass("has-error");
            $(element).closest('div').find(">.su-login-error").show();
        },
        unhighlight: function (element) {
            $(element).closest('.login-fields').removeClass('has-error');
            $(element).closest('div').find(">.su-login-error").hide();

        },
        errorElement: 'span',
        errorPlacement: function (error, element) {
            $(element).closest('div').find(">.su-login-error").attr("title", error.html());
        },
        messages: {
            username: {
                isRequired: "Username required"
            },
            password: {
                required: "Password required"
            }

        }
    });


});

function FormValidate() {
    ShowWaitingProgress(".login-container", "show");
    if ($('#login-form').valid())
        return true;
    else {
        ShowWaitingProgress(".login-container", "hide");
        return false;
    }

}