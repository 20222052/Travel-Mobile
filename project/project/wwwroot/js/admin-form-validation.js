/**
 * Admin Form Validation - Xử lý validate cho tất cả form trong Admin
 * Áp dụng cho: Create, Edit, Login forms
 */

(function () {
    'use strict';

    // Khởi tạo validation khi DOM load xong
    document.addEventListener('DOMContentLoaded', function () {
        initializeFormValidation();
    });

    function initializeFormValidation() {
        // Lấy tất cả các form cần validate
        const forms = document.querySelectorAll('.needs-validation');

        // Áp dụng validation cho từng form
        Array.prototype.forEach.call(forms, function (form) {
            form.addEventListener('submit', function (event) {
                // Xóa các thông báo lỗi cũ trước khi validate lại
                clearPreviousErrors(form);

                // Validate form
                if (!form.checkValidity() || !customValidation(form)) {
                    event.preventDefault();
                    event.stopPropagation();
                    
                    // Hiển thị thông báo lỗi
                    showValidationErrors(form);
                    
                    // Scroll đến field đầu tiên có lỗi
                    scrollToFirstError(form);
                }

                form.classList.add('was-validated');
            }, false);

            // Validate realtime khi user nhập liệu
            addRealtimeValidation(form);
        });
    }

    function customValidation(form) {
        let isValid = true;

        // Validate file upload (images)
        const fileInputs = form.querySelectorAll('input[type="file"][data-validate-image]');
        fileInputs.forEach(function (input) {
            if (input.files.length > 0) {
                const file = input.files[0];
                const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
                const maxSize = 5 * 1024 * 1024; // 5MB

                if (!allowedTypes.includes(file.type)) {
                    showCustomError(input, 'Chỉ chấp nhận file ảnh định dạng JPG, PNG, GIF');
                    isValid = false;
                } else if (file.size > maxSize) {
                    showCustomError(input, 'Kích thước file không được vượt quá 5MB');
                    isValid = false;
                }
            }
        });

        // Validate password confirmation
        const passwordInputs = form.querySelectorAll('input[type="password"][name="Password"]');
        passwordInputs.forEach(function (passwordInput) {
            const confirmInput = form.querySelector('input[name="ConfirmPassword"]');
            if (confirmInput && passwordInput.value !== confirmInput.value) {
                showCustomError(confirmInput, 'Mật khẩu xác nhận không khớp');
                isValid = false;
            }
        });

        // Validate email format
        const emailInputs = form.querySelectorAll('input[type="email"]');
        emailInputs.forEach(function (input) {
            if (input.value && !isValidEmail(input.value)) {
                showCustomError(input, 'Email không đúng định dạng');
                isValid = false;
            }
        });

        // Validate phone number
        const phoneInputs = form.querySelectorAll('input[data-validate-phone]');
        phoneInputs.forEach(function (input) {
            if (input.value && !isValidPhone(input.value)) {
                showCustomError(input, 'Số điện thoại không hợp lệ (10-11 số)');
                isValid = false;
            }
        });

        // Validate số dương (Price, View, People, Duration...)
        const numberInputs = form.querySelectorAll('input[type="number"][data-validate-positive]');
        numberInputs.forEach(function (input) {
            if (input.value && parseFloat(input.value) < 0) {
                showCustomError(input, 'Giá trị phải là số dương');
                isValid = false;
            }
        });

        // Validate date không được trong quá khứ (nếu cần)
        const futureDateInputs = form.querySelectorAll('input[type="date"][data-validate-future]');
        futureDateInputs.forEach(function (input) {
            if (input.value) {
                const selectedDate = new Date(input.value);
                const today = new Date();
                today.setHours(0, 0, 0, 0);
                
                if (selectedDate < today) {
                    showCustomError(input, 'Ngày không được trong quá khứ');
                    isValid = false;
                }
            }
        });

        return isValid;
    }

    function addRealtimeValidation(form) {
        const inputs = form.querySelectorAll('input, select, textarea');

        inputs.forEach(function (input) {
            // Validate khi user rời khỏi field
            input.addEventListener('blur', function () {
                validateField(this);
            });

            // Validate khi user đang nhập (cho một số trường đặc biệt)
            if (input.type === 'email' || input.type === 'password' || input.hasAttribute('data-validate-phone')) {
                input.addEventListener('input', function () {
                    if (this.classList.contains('is-invalid')) {
                        validateField(this);
                    }
                });
            }

            // Validate file ngay khi chọn
            if (input.type === 'file') {
                input.addEventListener('change', function () {
                    validateField(this);
                    previewImage(this);
                });
            }
        });

        // Validate password confirmation realtime
        const confirmPassword = form.querySelector('input[name="ConfirmPassword"]');
        if (confirmPassword) {
            confirmPassword.addEventListener('input', function () {
                const password = form.querySelector('input[name="Password"]');
                if (password && this.value) {
                    if (this.value !== password.value) {
                        showCustomError(this, 'Mật khẩu xác nhận không khớp');
                    } else {
                        clearFieldError(this);
                    }
                }
            });
        }
    }

    function validateField(field) {
        clearFieldError(field);

        // HTML5 validation
        if (!field.checkValidity()) {
            showFieldError(field, field.validationMessage);
            return false;
        }

        // Custom validation
        if (field.type === 'email' && field.value && !isValidEmail(field.value)) {
            showFieldError(field, 'Email không đúng định dạng');
            return false;
        }

        if (field.hasAttribute('data-validate-phone') && field.value && !isValidPhone(field.value)) {
            showFieldError(field, 'Số điện thoại không hợp lệ (10-11 số)');
            return false;
        }

        if (field.type === 'file' && field.hasAttribute('data-validate-image') && field.files.length > 0) {
            const file = field.files[0];
            const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];
            const maxSize = 5 * 1024 * 1024; // 5MB

            if (!allowedTypes.includes(file.type)) {
                showFieldError(field, 'Chỉ chấp nhận file ảnh định dạng JPG, PNG, GIF');
                return false;
            }
            if (file.size > maxSize) {
                showFieldError(field, 'Kích thước file không được vượt quá 5MB');
                return false;
            }
        }

        field.classList.remove('is-invalid');
        field.classList.add('is-valid');
        return true;
    }

    function showFieldError(field, message) {
        field.classList.add('is-invalid');
        field.classList.remove('is-valid');

        let errorDiv = field.parentElement.querySelector('.invalid-feedback');
        if (!errorDiv) {
            errorDiv = document.createElement('div');
            errorDiv.className = 'invalid-feedback';
            field.parentElement.appendChild(errorDiv);
        }
        errorDiv.textContent = message;
        errorDiv.style.display = 'block';
    }

    function clearFieldError(field) {
        field.classList.remove('is-invalid');
        field.classList.remove('is-valid');

        const errorDiv = field.parentElement.querySelector('.invalid-feedback');
        if (errorDiv) {
            errorDiv.style.display = 'none';
        }
    }

    function clearPreviousErrors(form) {
        const errorDivs = form.querySelectorAll('.invalid-feedback');
        errorDivs.forEach(function (div) {
            div.style.display = 'none';
        });

        const invalidFields = form.querySelectorAll('.is-invalid');
        invalidFields.forEach(function (field) {
            field.classList.remove('is-invalid');
        });
    }

    function showValidationErrors(form) {
        const invalidFields = form.querySelectorAll(':invalid');
        invalidFields.forEach(function (field) {
            validateField(field);
        });
    }

    function showCustomError(field, message) {
        showFieldError(field, message);
    }

    function scrollToFirstError(form) {
        const firstInvalid = form.querySelector('.is-invalid');
        if (firstInvalid) {
            firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
            firstInvalid.focus();
        }
    }

    function isValidEmail(email) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(email);
    }

    function isValidPhone(phone) {
        // Chấp nhận số điện thoại VN: 10-11 số, bắt đầu bằng 0
        const phoneRegex = /^0\d{9,10}$/;
        return phoneRegex.test(phone.replace(/\s/g, ''));
    }

    function previewImage(input) {
        if (input.files && input.files[0]) {
            const previewId = input.getAttribute('data-preview');
            if (previewId) {
                const preview = document.getElementById(previewId);
                if (preview) {
                    const reader = new FileReader();
                    reader.onload = function (e) {
                        preview.src = e.target.result;
                        preview.style.display = 'block';
                    };
                    reader.readAsDataURL(input.files[0]);
                }
            }
        }
    }

    // Export functions cho global scope (nếu cần)
    window.AdminValidation = {
        validateField: validateField,
        clearFieldError: clearFieldError,
        isValidEmail: isValidEmail,
        isValidPhone: isValidPhone
    };
})();
