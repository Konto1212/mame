#NO_APP
	.file	"main.c"
	.text
	.align	2
	.globl	main
	.type	main, @function
main:
	link.w %fp,#0
.L2:
	jsr VDP_waitVSync
	jra .L2
	.size	main, .-main
	.ident	"GCC: (GNU) 6.3.0"
