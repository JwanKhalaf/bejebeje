// client-side password validation for cognito password policy
(function () {
  const passwordInput = document.getElementById("password") || document.getElementById("newPassword");
  const submitBtn = document.getElementById("submit-btn");
  const checkLength = document.getElementById("check-length");
  const checkNumber = document.getElementById("check-number");
  const checkUppercase = document.getElementById("check-uppercase");
  const checkLowercase = document.getElementById("check-lowercase");

  if (!passwordInput || !submitBtn) return;

  const rules = [
    { element: checkLength, test: (v) => v.length >= 14 },
    { element: checkNumber, test: (v) => /\d/.test(v) },
    { element: checkUppercase, test: (v) => /[A-Z]/.test(v) },
    { element: checkLowercase, test: (v) => /[a-z]/.test(v) },
  ];

  const updateIndicator = (element, passed) => {
    if (!element) return;
    const dot = element.querySelector("span");
    if (passed) {
      dot.classList.remove("bg-neutral-600");
      dot.classList.add("bg-green-500");
      element.classList.remove("text-neutral-400");
      element.classList.add("text-green-400");
    } else {
      dot.classList.remove("bg-green-500");
      dot.classList.add("bg-neutral-600");
      element.classList.remove("text-green-400");
      element.classList.add("text-neutral-400");
    }
  };

  const validate = () => {
    const value = passwordInput.value;
    const allPassed = rules.every(({ element, test }) => {
      const passed = test(value);
      updateIndicator(element, passed);
      return passed;
    });

    // check if password match validation also exists on this page
    const matchCheck = window.passwordMatchValid;
    const matchRequired = document.getElementById("confirmPassword") !== null;

    if (matchRequired) {
      submitBtn.disabled = !(allPassed && matchCheck === true);
    } else {
      submitBtn.disabled = !allPassed;
    }

    // expose for password-match.js to use
    window.passwordPolicyValid = allPassed;
  };

  passwordInput.addEventListener("input", validate);

  // run once on load in case browser autofills
  validate();
})();
