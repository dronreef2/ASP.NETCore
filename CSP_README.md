# GitHub Pages CSP Fix
# This file helps resolve Content Security Policy issues

# If you still encounter CSP issues, you can:
# 1. Check the GitHub Actions workflow logs
# 2. Verify the generated HTML doesn't use eval() or similar functions
# 3. Ensure all CSS properties are supported by GitHub Pages

# Current CSP allows:
# - default-src 'self'
# - script-src 'self' 'unsafe-inline'
# - style-src 'self' 'unsafe-inline'
# - img-src 'self' data: https:
# - font-src 'self' https:
