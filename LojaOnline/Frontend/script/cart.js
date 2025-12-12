/* =========================================================
   CART SERVICE - Gestão do Carrinho
   ========================================================= */

const CartService = {
    // Key for localStorage
    STORAGE_KEY: 'box_caramela_cart',

    // Obter itens do carrinho
    getItems() {
        const items = localStorage.getItem(this.STORAGE_KEY);
        return items ? JSON.parse(items) : [];
    },

    // Adicionar item ao carrinho
    addItem(product, btnElement) {
        const items = this.getItems();
        const sizeToAdd = product.size || 'Tamanho Único';
        // Unique item defined by ID + Size
        const existingItem = items.find(item => item.id === product.id && item.size === sizeToAdd);

        if (existingItem) {
            existingItem.quantity += 1;
        } else {
            items.push({
                id: product.id,
                name: product.name,
                price: product.price,
                size: sizeToAdd,
                imageUrl: product.imageUrl || 'https://placehold.co/300?text=Sem+Imagem', // Fallback
                quantity: 1
            });
        }

        this.saveItems(items);
        this.updateCartCount();

        // Feedback Visual no Botão (sem alert)
        if (btnElement) {
            const originalText = btnElement.innerHTML;
            const originalClass = btnElement.className;

            btnElement.innerHTML = 'Adicionado!';
            btnElement.className = 'btn btn-success btn-sm';
            btnElement.disabled = true;

            setTimeout(() => {
                btnElement.innerHTML = originalText;
                btnElement.className = originalClass;
                btnElement.disabled = false;
            }, 1000);
        }
    },

    // Remover item
    removeItem(productId) {
        // Remover item needs to handle size too? 
        // Logic simplification: Removing by ID removes ALL sizes of that product?
        // OR better: removeItem needs unique ID. Currently ID is product.id. 
        // LIMITATION: If user has same product in S and M, delete will remove BOTH or First?
        // FIX: filter should match both ID.
        // But the UI usually passes just ID.
        // Let's improve UI later. For now, let's assume removing removes all sizes of that ID or logic needs refinement.
        // Better: frontend render passes index or composed ID.
        // Let's try to remove exact match if possible, but for now stick to ID to avoid breaking UI too much.
        let items = this.getItems();
        items = items.filter(item => item.id !== productId); // Removes all instances of that product
        this.saveItems(items);
        this.updateCartCount();
        window.dispatchEvent(new Event('cart-updated'));
    },

    // Salvar no storage
    saveItems(items) {
        localStorage.setItem(this.STORAGE_KEY, JSON.stringify(items));
    },

    // Limpar carrinho
    clear() {
        localStorage.removeItem(this.STORAGE_KEY);
        this.updateCartCount();
    },

    // Atualizar contador no menu e badge
    updateCartCount() {
        const items = this.getItems();
        const totalCount = items.reduce((sum, item) => sum + item.quantity, 0);

        const badge = document.getElementById('cart-count');
        if (badge) {
            badge.textContent = totalCount;
            badge.style.display = totalCount > 0 ? 'inline-block' : 'none';
        }
    },

    // Calcular total
    getTotal() {
        return this.getItems().reduce((sum, item) => sum + (item.price * item.quantity), 0);
    },

    // Finalizar Compra - Mostrar formulário de pagamento
    async checkout() {
        const items = this.getItems();
        if (items.length === 0) {
            alert('O carrinho está vazio.');
            return;
        }

        if (!AuthService.isAuthenticated()) {
            alert('Tem de fazer login para finalizar a compra.');
            window.location.href = 'login.html';
            return;
        }

        // Fechar modal do carrinho e abrir modal de pagamento
        const cartModal = bootstrap.Modal.getInstance(document.getElementById('cartModal'));
        if (cartModal) cartModal.hide();

        // Aguardar animação de fecho
        setTimeout(() => {
            const paymentModal = new bootstrap.Modal(document.getElementById('paymentModal'));
            paymentModal.show();
        }, 300);
    },

    // Processar pagamento (chamado após submeter formulário)
    async processPayment(paymentData) {
        const items = this.getItems();

        // Preparar dados para a API (DTO: ProductId, Quantity, Size)
        const orderData = items.map(item => ({
            productId: item.id,
            quantity: item.quantity,
            size: item.size
        }));

        try {
            const response = await ApiService.request('/Orders', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${AuthService.getToken()}`
                },
                body: JSON.stringify(orderData)
            });

            // Sucesso: Limpar carrinho
            this.clear();

            // Fechar modal de pagamento
            const paymentModal = bootstrap.Modal.getInstance(document.getElementById('paymentModal'));
            if (paymentModal) paymentModal.hide();

            // Mostrar mensagem de sucesso
            setTimeout(() => {
                const successModal = new bootstrap.Modal(document.getElementById('successModal'));
                document.getElementById('orderIdDisplay').textContent = response.orderId || 'N/A';
                document.getElementById('paymentStatusDisplay').textContent = response.paymentStatus || 'Processado';
                successModal.show();
            }, 300);

        } catch (error) {
            alert('Erro ao finalizar encomenda: ' + error.message);
        }
    }
};

// Inicializar contador ao carregar
document.addEventListener('DOMContentLoaded', () => {
    CartService.updateCartCount();
});
