$(document).ready(function() {

    // --- Globals & Initial Setup ---
    let cart = JSON.parse(localStorage.getItem('shoppingCart')) || [];
    const productListContainer = $('#product-list');
    const cartItemsContainer = $('#cart-items');
    const cartTotalSpan = $('#cart-total');
    const cartCountBadge = $('#cart-count');
    const userGreeting = $('#user-greeting');

    // Function to update Navbar based on login status
    function updateNavbarUI() {
        const loggedInUser = localStorage.getItem('loggedInUser');
        const userName = localStorage.getItem('loggedInUserName'); // Get user's name

        if (loggedInUser) {
            $('#signin-nav-item').hide();
            $('#signup-nav-item').hide();
            $('#logout-nav-item').show();
            userGreeting.text(`Hi, ${userName || 'User'}!`); // Display name or default
        } else {
            $('#signin-nav-item').show();
            $('#signup-nav-item').show();
            $('#logout-nav-item').hide();
            userGreeting.text('');
        }
        updateCartCount(); // Update cart count on navbar change too
    }

    // Function to save cart to localStorage
    function saveCart() {
        localStorage.setItem('shoppingCart', JSON.stringify(cart));
        updateCartCount();
        updateCartDisplay(); // Update modal display whenever cart changes
    }

    // Function to update cart item count badge
    function updateCartCount() {
        let totalItems = 0;
        cart.forEach(item => {
            totalItems += item.quantity;
        });
        cartCountBadge.text(totalItems);
    }

    // Function to update the cart modal display
    function updateCartDisplay() {
        cartItemsContainer.empty(); // Clear previous items
        let total = 0;

        if (cart.length === 0) {
            cartItemsContainer.html('<p>Your cart is empty.</p>');
            cartTotalSpan.text('0.00');
            return;
        }

        const ul = $('<ul class="list-group"></ul>');
        cart.forEach((item, index) => {
            const itemTotal = item.price * item.quantity;
            total += itemTotal;
            const li = $(`
                <li class="list-group-item d-flex justify-content-between align-items-center">
                    <div>
                        <strong>${item.name}</strong><br>
                        <small>$${item.price.toFixed(2)} x ${item.quantity}</small>
                    </div>
                    <div>
                        <span class="me-3">\$${itemTotal.toFixed(2)}</span>
                        <button class="btn btn-sm btn-outline-secondary decrease-qty" data-index="${index}">-</button>
                        <span class="mx-1">${item.quantity}</span>
                        <button class="btn btn-sm btn-outline-secondary increase-qty" data-index="${index}">+</button>
                        <button class="btn btn-sm btn-danger remove-item ms-2" data-index="${index}">×</button>
                    </div>
                </li>
            `);
            ul.append(li);
        });

        cartItemsContainer.append(ul);
        cartTotalSpan.text(total.toFixed(2));
    }

    // --- Product Loading (index.html) ---
    if (productListContainer.length) { // Check if the product list container exists on the page
        $.getJSON('https://fakestoreapi.com/products', function(products) {
            productListContainer.empty(); // Clear spinner/loading message
            products.forEach(product => {
                const productCard = `
                    <div class="col-md-6 col-lg-4 mb-4">
                        <div class="card h-100">
                            <img src="${product.image}" class="card-img-top" alt="${product.title}">
                            <div class="card-body d-flex flex-column">
                                <h5 class="card-title">${product.title}</h5>
                                <p class="card-text flex-grow-1">${product.description}</p>
                                <p class="card-text"><strong>Price: $${product.price.toFixed(2)}</strong></p>
                                <button class="btn btn-success mt-auto add-to-cart"
                                        data-id="${product.id}"
                                        data-name="${product.title}"
                                        data-price="${product.price}">Add to Cart</button>
                            </div>
                        </div>
                    </div>
                `;
                productListContainer.append(productCard);
            });
        }).fail(function(jqXHR, textStatus, errorThrown) {
            console.error("Error loading products.json:", textStatus, errorThrown);
            productListContainer.html('<p class="text-danger">Could not load products. Please try again later.</p>');
        });
    }

    // --- Event Handlers ---

    // Add to Cart (Event Delegation for dynamically loaded buttons)
    $('body').on('click', '.add-to-cart', function() {
        const button = $(this);
        const productId = button.data('id');
        const productName = button.data('name');
        const productPrice = parseFloat(button.data('price'));

        // Check if item already in cart
        const existingItemIndex = cart.findIndex(item => item.id === productId);

        if (existingItemIndex > -1) {
            cart[existingItemIndex].quantity += 1;
        } else {
            cart.push({
                id: productId,
                name: productName,
                price: productPrice,
                quantity: 1
            });
        }

        saveCart();

        // Optional: Provide user feedback
        button.text('Added!').addClass('btn-success').removeClass('btn-primary');
        setTimeout(() => {
             button.text('Add to Cart').removeClass('btn-success').addClass('btn-primary');
        }, 1000); // Reset button after 1 second
    });

    // Update Cart display when Modal is shown
    $('#cartModal').on('show.bs.modal', function () {
        updateCartDisplay();
    });

    // Clear Cart
    $('#clear-cart-btn').on('click', function() {
        if(confirm('Are you sure you want to clear the entire cart?')) {
            cart = [];
            saveCart();
        }
    });

     // Checkout Button (Simple Alert)
    $('#checkout-btn').on('click', function() {
        if (cart.length > 0) {
            alert(`Thank you for your purchase! Your total is $${cartTotalSpan.text()}. (This is a demo - no actual transaction occurred).`);
            cart = []; // Clear cart after "checkout"
            saveCart();
            $('#cartModal').modal('hide'); // Hide the modal
        } else {
            alert('Your cart is empty.');
        }
    });

    // Cart Item Quantity/Remove (Event Delegation inside Modal)
    cartItemsContainer.on('click', '.remove-item', function() {
        const index = $(this).data('index');
        cart.splice(index, 1); // Remove item from array by index
        saveCart();
    });

    cartItemsContainer.on('click', '.increase-qty', function() {
        const index = $(this).data('index');
        cart[index].quantity += 1;
        saveCart();
    });

    cartItemsContainer.on('click', '.decrease-qty', function() {
        const index = $(this).data('index');
        if (cart[index].quantity > 1) {
            cart[index].quantity -= 1;
        } else {
            // Optionally remove if quantity becomes 0, or just leave it at 1
            cart.splice(index, 1); // Remove item if quantity reaches 0
        }
        saveCart();
    });


    // Registration Form Submission (register.html)
    $('#register-form').on('submit', function(event) {
        event.preventDefault(); // Prevent actual form submission

        // Clear previous errors/success messages
        $('#register-error').hide();
        $('#register-success').hide();
        $('#register-confirm-password').removeClass('is-invalid');

        const name = $('#register-name').val().trim();
        const email = $('#register-email').val().trim();
        const password = $('#register-password').val();
        const confirmPassword = $('#register-confirm-password').val();

        // Basic Validation
        if (!name || !email || !password || !confirmPassword) {
             $('#register-error').text('Please fill in all fields.').show();
             return;
        }
        if (password !== confirmPassword) {
            $('#register-confirm-password').addClass('is-invalid');
             $('#register-error').text('Passwords do not match.').show();
            return;
        }

        // --- Simulation: Store user data in localStorage ---
        // In a real app, this would be an AJAX call to a server
        let users = JSON.parse(localStorage.getItem('users')) || [];

        // Check if email already exists
        if (users.some(user => user.email === email)) {
            $('#register-error').text('Email address already registered.').show();
            return;
        }

        // Add new user (NOTE: Storing passwords directly in localStorage is INSECURE for real apps!)
        users.push({ name: name, email: email, password: password }); // In real app, hash password server-side
        localStorage.setItem('users', JSON.stringify(users));

        $('#register-success').show();
        // Redirect to login after a short delay
        setTimeout(function() {
             window.location.href = 'login.html';
        }, 2000); // Redirect after 2 seconds
    });


    // Login Form Submission (login.html)
    $('#login-form').on('submit', function(event) {
        event.preventDefault();
        $('#login-error').hide();

        const email = $('#login-email').val().trim();
        const password = $('#login-password').val();

        if (!email || !password) {
            $('#login-error').text('Please enter email and password.').show();
            return;
        }

        // --- Simulation: Check user data in localStorage ---
        let users = JSON.parse(localStorage.getItem('users')) || [];
        const foundUser = users.find(user => user.email === email && user.password === password); // Simple password check (INSECURE!)

        if (foundUser) {
            // Login successful
            localStorage.setItem('loggedInUser', foundUser.email);
            localStorage.setItem('loggedInUserName', foundUser.name); // Store the name
            window.location.href = 'index.html'; // Redirect to home page
        } else {
            // Login failed
            $('#login-error').text('Invalid email or password.').show();
        }
    });

    // Logout Button
    $('#logout-btn').on('click', function() {
        localStorage.removeItem('loggedInUser');
        localStorage.removeItem('loggedInUserName');
        updateNavbarUI(); 
    });

    // --- Initial Page Load ---
    updateNavbarUI(); 


}); 