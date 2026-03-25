// client-side password validation for cognito password policy
// uses svg icon toggle pattern (x-circle / check-circle) with
// container border colour change when all criteria are met
(function () {
  const passwordInput = document.getElementById("password") || document.getElementById("newPassword");
  const confirmPasswordInput = document.getElementById("confirmPassword");
  const submitBtn = document.getElementById("submit-btn");
  const validationContainer = document.getElementById("password-validation");

  if (!passwordInput || !submitBtn || !validationContainer) return;

  const rules = [
    {
      id: "length",
      test: function (v) { return v.length >= 14; },
    },
    {
      id: "number",
      test: function (v) { return /\d/.test(v); },
    },
    {
      id: "uppercase",
      test: function (v) { return /[A-Z]/.test(v); },
    },
    {
      id: "lowercase",
      test: function (v) { return /[a-z]/.test(v); },
    },
  ];

  var policyValid = false;
  var matchValid = !confirmPasswordInput;

  var updateIcon = function (id, passed) {
    var invalidIcon = document.getElementById(id + "-icon-invalid");
    var validIcon = document.getElementById(id + "-icon-valid");
    var text = document.getElementById(id + "-text");

    if (!invalidIcon || !validIcon || !text) return;

    invalidIcon.style.display = passed ? "none" : "block";
    validIcon.style.display = passed ? "block" : "none";
    text.className = passed
      ? "text-sm font-medium text-green-300"
      : "text-sm font-medium text-red-300";
  };

  var updateContainer = function () {
    var allValid = policyValid && matchValid;

    if (allValid) {
      validationContainer.className = "border rounded-md border-green-500/30 bg-green-500/10 p-3 mt-3";
    } else {
      validationContainer.className = "border rounded-md border-red-500/30 bg-red-500/10 p-3 mt-3";
    }

    submitBtn.disabled = !allValid;
  };

  var validatePolicy = function () {
    var value = passwordInput.value;

    policyValid = rules.every(function (rule) {
      var passed = rule.test(value);
      updateIcon(rule.id, passed);
      return passed;
    });

    // if there is a confirm password field, also check match
    if (confirmPasswordInput) {
      validateMatch();
    }

    updateContainer();
  };

  var validateMatch = function () {
    var pw = passwordInput.value;
    var cpw = confirmPasswordInput.value;

    matchValid = pw.length > 0 && pw === cpw;
    updateIcon("match", matchValid);
    updateContainer();
  };

  passwordInput.addEventListener("input", validatePolicy);

  if (confirmPasswordInput) {
    confirmPasswordInput.addEventListener("input", validateMatch);
  }

  // run once on load
  validatePolicy();
})();
