describe('Catalogs', function () {

    beforeEach(() => {
        cy.visit('/');
    });
    
    it('should display catalogs', function () {
        cy.get('[data-cy=catalog-name]').eq(1).should('contain', 'T');
    });
});