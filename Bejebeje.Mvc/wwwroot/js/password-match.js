// client-side password match validation for reset password page
(function () {
  const newPasswordInput = document.getElementById("newPassword");
  const confirmPasswordInput = document.getElementById("confirmPassword");
  const matchMessage = document.getElementById("password-match");
  const submitBtn = document.getElementById("submit-btn");

  if (!newPasswordInput || !confirmPasswordInput || !submitBtn) return;

  const validate = () => {
    const newVal = newPasswordInput.value;
    const confirmVal = confirmPasswordInput.value;

    if (confirmVal.length === 0) {
      matchMessage.classList.add("hidden");
      window.passwordMatchValid = false;
    } else if (newVal !== confirmVal) {
      matchMessage.classList.remove("hidden");
      matchMessage.classList.remove("text-green-400");
      matchMessage.classList.add("text-red-400");
      matchMessage.textContent = "Passwords do not match";
      window.passwordMatchValid = false;
    } else {
      matchMessage.classList.remove("hidden");
      matchMessage.classList.remove("text-red-400");
      matchMessage.classList.add("text-green-400");
      matchMessage.textContent = "Passwords match";
      window.passwordMatchValid = true;
    }

    // update submit button state (password-validation.js checks window.passwordPolicyValid)
    const policyValid = window.passwordPolicyValid === true;
    const matchValid = window.passwordMatchValid === true;
    submitBtn.disabled = !(policyValid && matchValid);
  };

  newPasswordInput.addEventListener("input", validate);
  confirmPasswordInput.addEventListener("input", validate);

  validate();
})();
