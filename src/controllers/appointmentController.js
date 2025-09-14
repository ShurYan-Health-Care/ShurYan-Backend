const appointmentService = require('../services/appointmentService');

const createAppointment = async (req, res) => {
  try {
    const appointment = await appointmentService.create(req.body);
    res.status(201).json({
      success: true,
      data: appointment
    });
  } catch (error) {
    res.status(400).json({
      success: false,
      error: error.message
    });
  }
};

module.exports = {
  createAppointment
};
